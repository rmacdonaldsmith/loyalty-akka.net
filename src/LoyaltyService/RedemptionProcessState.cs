using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private Guid _redemptionId;
        private long _gpid;
        private States _currentState = States.None;
        private bool _passedFraudCheck;

        public RedemptionProcessState()
        {
            StartRedemptionProcess();
        }

        private void StartRedemptionProcess()
        {
            Receive<Messages.Commands.StartOTGiftCardRedemption>(msg =>
            {
                _redemptionId = Guid.NewGuid();
                _currentState = States.StartingRedemption;
                Sender.Tell(new Messages.Events.OTGiftCardRedemptionStarted(msg.Gpid));
                //_processManager.Tell(new Messages.Events.OTGiftCardRedemptionStarted(msg.Gpid));
                Become(WaitingForFraudCheck);
            });
        }

        private void WaitingForFraudCheck()
        {
            Receive<Messages.Events.FraudCheckPassed>(passed =>
                {
                    _passedFraudCheck = true;
                    Context.ActorSelection("/user/redemptionController/redemptionProcessManager").Tell(new Messages.Events.OTGiftCardRedemptionStarted(passed.Gpid));
                
                    //_processManager.Tell(new Messages.Commands.CheckPointsBalance(passed.Gpid));
                    Become(WaitingForPointsBalance);
                });

        }

        private void WaitingForPointsBalance()
        {
             
        }
    }
}
