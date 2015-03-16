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
        CheckingPointsBalance,
        OrderingGiftCard,
        GiftCardOrdered,
        None
    }

    public class RedemptionProcessState : ReceiveActor
    {
        private readonly ActorRef _processManager;
        private Guid _redemptionId;
        private long _gpid;
        private States _currentState = States.None;
        private bool _passedFraudCheck;

        public RedemptionProcessState(ActorRef processManager)
        {
            _processManager = processManager;

            Receive<Messages.Commands.StartOTGiftCardRedemption>(msg =>
                {
                    _redemptionId = Guid.NewGuid();
                    _currentState = States.StartingRedemption;
                    _processManager.Tell(new Messages.Events.OTGiftCardRedemptionStarted(msg.Gpid));
                    Become(m =>
                        {
                            if (m is Messages.Events.FraudCheckPassed)
                            {

                            }
                        });
                });



        }

        private void HandleFraudCheckPassed(Messages.Events.FraudCheckPassed passed)
        {
            _passedFraudCheck = true;
            Become((msg) =>
                {
                    
                });
        }
    }
}
