using System;
using Akka.Actor;
using Akka.TestKit.Xunit;
using LoyaltyService.FraudDetection;
using Xunit;

namespace LoyaltyService.Tests
{
    public class RedemptionProcessManagerStateTests : TestKit
    {
        [Fact]
        public void Should_not_handle_messages_until_Start_is_received()
        {
            var pmStateActor = ActorOfAsTestActorRef<RedemptionProcessState>();
            pmStateActor.Tell(new SiftServiceActor.FraudCheckPassed(123));
            ExpectNoMsg();

            pmStateActor.Tell(
                new Messages.Commands.StartOTGiftCardRedemption(1234, "USD", 1000, "user@address.com"));
            ExpectMsg<Messages.Events.OTGiftCardRedemptionStarted>();
        }
    }
}
