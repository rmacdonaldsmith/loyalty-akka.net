using System;
using System.Threading;
using Akka.Actor;

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

        private void StartRedemptionProcess()
        {
            Receive<Messages.Commands.StartOTGiftCardRedemption>(msg =>
            {
                _redemptionId = Guid.NewGuid();
                _currentState = States.StartingRedemption;
                _pointsRequired = msg.PointsAmount;
                _userEmail = msg.UserEmail;
                _ccy = msg.CCY;
                _processBroker.Tell(new Messages.Events.OTGiftCardRedemptionStarted(msg.Gpid));
                Context.System.Scheduler.ScheduleOnce(TimeSpan.FromSeconds(1), Self, new FraudCheckTimedOut(),
                                                      _timeoutCancellation);
                Become(WaitingForFraudCheck);
            });
        }

        private void WaitingForFraudCheck()
        {
            //TODO: how to handle timeouts?
            // use scheduler?
            Receive<Messages.Events.FraudCheckPassed>(passed =>
                {
                    _passedFraudCheck = true;
                    _processBroker.Tell(new Messages.Commands.CheckPointsBalance(passed.Gpid));
                    Become(WaitingForPointsBalance);
                });

            Receive<FraudCheckTimedOut>(timedout =>
                {
                    //what do we want to do? retry?
                });
        }

        private void WaitingForPointsBalance()
        {
            Receive<Messages.Events.PointsBalanceResult>(points =>
                {
                    if (points.PointsBalance >= _pointsRequired)
                    {
                        _processBroker.Tell(new Messages.Commands.OrderOtGiftCard(_gpid, _userEmail, _pointsRequired, _ccy)); //tell the broker to order the gift card
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
