using System;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Xunit;

namespace LoyaltyService.Tests
{
    public class RedemptionProcessManagerStateTests : TestKit
    {
        [Fact]
        public void Should_not_handle_messages_until_Start_is_received()
        {
            var stateActorProps = Props.Create(() => new RedemptionProcessStateActor(TestActor));
            var pmStateActor = ActorOf(stateActorProps, "state-process-manager");
            pmStateActor.Tell(new Events.FraudCheckPassed(123, Guid.NewGuid()));
            ExpectNoMsg();

            pmStateActor.Tell(
                new Commands.StartOTGiftCardRedemption(1234, Guid.NewGuid(), "USD", 1000, "user@address.com"));
            ExpectMsg<Events.OTGiftCardRedemptionStarted>();
        }

        [Fact(DisplayName = "When the sift score is too low")]
        public void When_sift_score_is_too_low()
        {
            var stateActorProps = Props.Create(() => new RedemptionProcessStateActor(TestActor));
            var pmStateActor = ActorOf(stateActorProps, "state-process-manager");
            pmStateActor.Tell(
                new Commands.StartOTGiftCardRedemption(
                    1234, Guid.NewGuid(), "USD", 1000, "user@address.com"));
            pmStateActor.Tell(new Events.SiftScore(1234, Guid.NewGuid(), 10));

            ExpectMsg<Events.OTGiftCardRedemptionStarted>();
            ExpectMsg<Events.FraudCheckFailed>();
        }

        [Fact(DisplayName = "When the sift score is not quite high enough")]
        public void When_sift_score_is_not_high_enough()
        {
            var stateActorProps = Props.Create(() => new RedemptionProcessStateActor(TestActor));
            var pmStateActor = ActorOf(stateActorProps, "state-process-manager");
            pmStateActor.Tell(
                new Commands.StartOTGiftCardRedemption(
                    1234, Guid.NewGuid(), "USD", 1000, "user@address.com"));
            pmStateActor.Tell(new Events.SiftScore(1234, Guid.NewGuid(), 60));

            ExpectMsg<Events.OTGiftCardRedemptionStarted>();
            ExpectMsg<Events.FraudCheckPendingManualReview>();
        }

        [Fact(DisplayName = "When the sift score is high")]
        public void When_sift_score_is_good()
        {
            var stateActorProps = Props.Create(() => new RedemptionProcessStateActor(TestActor));
            var pmStateActor = ActorOf(stateActorProps, "state-process-manager");
            pmStateActor.Tell(
                new Commands.StartOTGiftCardRedemption(
                    1234, Guid.NewGuid(), "USD", 1000, "user@address.com"));
            pmStateActor.Tell(new Events.SiftScore(1234, Guid.NewGuid(), 100));

            ExpectMsg<Events.OTGiftCardRedemptionStarted>();
            ExpectMsg<Events.FraudCheckPassed>();
        }

        [Fact(DisplayName = "When the user has a good fraud score and enough points")]
        public void When_points_balance_ok()
        {
            const int gpid = 1234;
            const int requiredPoints = 1000;
            var stateActorProps = Props.Create(() => new RedemptionProcessStateActor(TestActor));
            var pmStateActor = ActorOf(stateActorProps, "state-process-manager");
            pmStateActor.Tell(
                new Commands.StartOTGiftCardRedemption(
                    gpid, Guid.NewGuid(), "USD", requiredPoints, "user@address.com"));
            pmStateActor.Tell(new Events.SiftScore(gpid, Guid.NewGuid(), 100));
            pmStateActor.Tell(new Events.PointsBalanceResult(gpid, Guid.NewGuid(), requiredPoints + 1000));
            pmStateActor.Tell(new Events.OtGiftCardOrdered(gpid, Guid.NewGuid(), "confirmation-number"));

            ExpectMsg<Events.OTGiftCardRedemptionStarted>();
            ExpectMsg<Events.FraudCheckPassed>();
            ExpectMsg<Commands.CheckPointsBalance>();
            ExpectMsg<Commands.OrderOtGiftCard>();
            ExpectMsg<Commands.NotifyUser>();
            ExpectMsg<Events.GiftCardOrdered>();
        }
    }
}
