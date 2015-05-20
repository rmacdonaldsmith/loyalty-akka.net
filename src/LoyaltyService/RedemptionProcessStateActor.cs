using System;
using System.Threading;
using Akka.Actor;

namespace LoyaltyService
{
    public class RedemptionProcessStateActor : ReceiveActor
    {
        private readonly IActorRef _processBroker;
        private Guid _redemptionId;
        private long _gpid;
        private int _pointsRequired;
        private string _userEmail;
        private string _ccy;
        private States _currentState = States.None;
        private bool _passedFraudCheck;
        private readonly CancellationToken _timeoutCancellation = new CancellationToken();

        public class FraudCheckTimedOut
        {
            //empty?
        }

        public RedemptionProcessStateActor(IActorRef processBroker)
        {
            _processBroker = processBroker;
            StartRedemptionProcess();
        }

        public RedemptionProcessStateActor()
        {
            //required for testkit
        }

        private void StartRedemptionProcess()
        {
            Receive<Commands.StartOTGiftCardRedemption>(msg =>
            {
                _redemptionId = Guid.NewGuid();
                _currentState = States.StartingRedemption;
                _pointsRequired = msg.PointsAmount;
                _userEmail = msg.UserEmail;
                _ccy = msg.CCY;
                _processBroker.Tell(new Events.OTGiftCardRedemptionStarted(msg.Gpid, _redemptionId, msg.UserEmail, msg.PointsAmount, msg.CCY));
                Context.System.Scheduler.ScheduleOnce(TimeSpan.FromSeconds(1), Self, new FraudCheckTimedOut(),
                                                      _timeoutCancellation);
                Become(WaitingForFraudCheck);
            });
        }

        private void WaitingForFraudCheck()
        {
            //TODO: handle timeouts with the scheduler
            Receive<Events.SiftScore>(score =>
                {
                    if (score.Score < 50)
                        _processBroker.Tell(new Events.FraudCheckFailed(score.Gpid, score.RedemptionId, "Score too low"));
                    else if (score.Score < 70)
                        _processBroker.Tell(new Events.FraudCheckPendingManualReview(score.Gpid, score.RedemptionId, "Score not quite high enough"));
                    else
                        _processBroker.Tell(new Events.FraudCheckPassed(score.Gpid, score.RedemptionId));
                    
                    _passedFraudCheck = true; //do we need to persist state like this?
                    _processBroker.Tell(new Commands.CheckPointsBalance(score.Gpid, score.RedemptionId));
                    Become(WaitingForPointsBalance);
                });

            Receive<FraudCheckTimedOut>(timedout =>
                {
                    //what do we want to do? retry? incremental back off?
                });
        }

        private void WaitingForPointsBalance()
        {
            Receive<Events.PointsBalanceResult>(points =>
                {
                    if (points.PointsBalance >= _pointsRequired)
                    {
                        _processBroker.Tell(new Commands.OrderOtGiftCard(_gpid, points.RedemptionId, _userEmail, _pointsRequired, _ccy)); //tell the broker to order the gift card
                        Become(WaitingForGiftOrderConfirmation);
                    }
                    else
                    {
                        _processBroker.Tell(new Events.InsufficientPoints(_gpid, points.RedemptionId,
                                                                          points.PointsBalance, _pointsRequired));
                    }
                });

            //TODO: timeout
        }

        private void WaitingForGiftOrderConfirmation()
        {
            Receive<Events.OtGiftCardOrdered>(
                ordered =>
                _processBroker.Tell(new Commands.NotifyUser(ordered.Gpid, ordered.RedemptionId, _userEmail,
                                                            "Confirmation number: " + ordered.ConfirmationNumber)));

            Receive<Events.OtGiftCardOrderFailed>(failed =>
                {
                    //retry?
                });
        }

        private void WaitingForTmsConfirmation()
        {
            //we are done - what else do we need to do here before we exit and die
            //tell the RedemptionController that we completed so it can clean up
            Receive<Events.UserNotified>(
                notified =>
                _processBroker.Tell(new Events.RedemptionCompleted(_gpid, _redemptionId)));
        }
    }
}
