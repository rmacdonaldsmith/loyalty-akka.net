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
                _processBroker.Tell(new RedemptionController.OTGiftCardRedemptionStarted(msg.Gpid));
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
                    
                    _passedFraudCheck = true;
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
            Receive<GiftService.OtGiftCardOrdered>(ordered =>
                {
                    //now we need to die; the order process has completed
                });

            Receive<GiftService.OtGiftCardOrderFailed>(failed =>
                {
                    //retry?
                });
        }
    }
}
