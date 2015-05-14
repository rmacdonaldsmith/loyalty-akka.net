using System;
using System.Threading;
using Akka.Actor;
using LoyaltyService.FraudDetection;

namespace LoyaltyService
{
    public enum States
    {
        StartingRedemption,
        PerformingFraudCheck,
        FraudCheckPassed,
        CheckingPointsBalance,
        OrderingGiftCard,
        GiftCardOrdered,
        None
    }

    public class RedemptionProcessState : ReceiveActor
    {
        private readonly ActorRef _processBroker;
        private Guid _redemptionId;
        private long _gpid;
        private int _pointsRequired;
        private string _userEmail;
        private string _ccy;
        private States _currentState = States.None;
        private bool _passedFraudCheck;
        private CancellationToken _timeoutCancellation = new CancellationToken();

        #region messages

        public class FraudCheckTimedOut
        {
            //empty?
        }

        public class InsufficientPoints
        {
            public long GPID { get; set; }
            public int PointsRequired { get; set; }
            public int PointsBalance { get; set; }
        }

        public class GiftCardOrdered : Messages.RedemptionBase
        {
            public string ConfirmationNumber { get; set; }

            public GiftCardOrdered(long gpid, string confirmationNumber) 
                : base(gpid)
            {
                ConfirmationNumber = confirmationNumber;
            }
        }

        #endregion

        public RedemptionProcessState(ActorRef processBroker)
        {
            _processBroker = processBroker;
            StartRedemptionProcess();
        }

        public RedemptionProcessState()
        {
            //required for testkit
        }

        private void StartRedemptionProcess()
        {
            Receive<RedemptionController.StartOTGiftCardRedemption>(msg =>
            {
                _redemptionId = Guid.NewGuid();
                _currentState = States.StartingRedemption;
                _pointsRequired = msg.PointsAmount;
                _userEmail = msg.UserEmail;
                _ccy = msg.CCY;
                _processBroker.Tell(new RedemptionController.OTGiftCardRedemptionStarted(msg.Gpid, msg.UserEmail, msg.PointsAmount, msg.CCY));
                Context.System.Scheduler.ScheduleOnce(TimeSpan.FromSeconds(1), Self, new FraudCheckTimedOut(),
                                                      _timeoutCancellation);
                Become(WaitingForFraudCheck);
            });
        }

        private void WaitingForFraudCheck()
        {
            //TODO: handle timeouts with the scheduler
            Receive<SiftServiceActor.SiftScore>(score =>
                {
                    if (score.Score < 50)
                        _processBroker.Tell(new FraudCheckerActor.FraudCheckFailed(score.Gpid, "Score too low"));
                    else if (score.Score < 70)
                        _processBroker.Tell(new FraudCheckerActor.FraudCheckPendingManualReview(score.Gpid, "Score not quite high enough"));
                    else
                        _processBroker.Tell(new FraudCheckerActor.FraudCheckPassed(score.Gpid));
                    
                    _passedFraudCheck = true; //do we need to persist state like this?
                    _processBroker.Tell(new PointsService.CheckPointsBalance(score.Gpid));
                    Become(WaitingForPointsBalance);
                });

            Receive<FraudCheckTimedOut>(timedout =>
                {
                    //what do we want to do? retry? incremental back off?
                });
        }

        private void WaitingForPointsBalance()
        {
            Receive<PointsService.PointsBalanceResult>(points =>
                {
                    if (points.PointsBalance >= _pointsRequired)
                    {
                        _processBroker.Tell(new GiftService.OrderOtGiftCard(_gpid, _userEmail, _pointsRequired, _ccy)); //tell the broker to order the gift card
                        Become(WaitingForGiftOrderConfirmation);
                    }
                    else
                    {
                        _processBroker.Tell(new InsufficientPoints
                            {
                                GPID = _gpid,
                                PointsBalance = points.PointsBalance,
                                PointsRequired = _pointsRequired,
                            });
                    }
                });

            //TODO: timeout
        }

        private void WaitingForGiftOrderConfirmation()
        {
            Receive<GiftService.OtGiftCardOrdered>(
                ordered => 
                    _processBroker.Tell(new TmsService.NotifyUser(ordered.Gpid, _userEmail, "Confirmation number: " + ordered.ConfirmationNumber)));

            Receive<GiftService.OtGiftCardOrderFailed>(failed =>
                {
                    //retry?
                });
        }

        private void WaitingForTmsConfirmation()
        {
            //we are done - what else do we need to do here before we exit and die
            //tell the RedemptionController that we completed so it can clean up
            Receive<TmsService.UserNotified>(
                notified =>
                _processBroker.Tell(new RedemptionController.RedemptionCompleted(_gpid, _redemptionId)));
        }
    }
}
