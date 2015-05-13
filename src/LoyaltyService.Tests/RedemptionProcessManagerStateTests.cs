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
            var stateActorProps = Props.Create(() => new RedemptionProcessState(TestActor));
            var pmStateActor = ActorOf(stateActorProps, "state-process-manager");
            pmStateActor.Tell(new FraudCheckerActor.FraudCheckPassed(123));
            ExpectNoMsg();

            pmStateActor.Tell(
                new RedemptionController.StartOTGiftCardRedemption(1234, "USD", 1000, "user@address.com"));
            ExpectMsg<RedemptionController.OTGiftCardRedemptionStarted>();
        }

        [Fact(DisplayName = "When the sift score is too low")]
        public void When_sift_score_is_too_low()
        {
            var stateActorProps = Props.Create(() => new RedemptionProcessState(TestActor));
            var pmStateActor = ActorOf(stateActorProps, "state-process-manager");
            pmStateActor.Tell(
                new RedemptionController.StartOTGiftCardRedemption(
                    1234, "USD", 1000, "user@address.com"));
            pmStateActor.Tell(new SiftServiceActor.SiftScore(1234, 10));

            ExpectMsg<RedemptionController.OTGiftCardRedemptionStarted>();
            ExpectMsg<FraudCheckerActor.FraudCheckFailed>();
        }
    }
}
