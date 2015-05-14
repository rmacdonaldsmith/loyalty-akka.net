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

        [Fact(DisplayName = "When the sift score is not quite high enough")]
        public void When_sift_score_is_not_high_enough()
        {
            var stateActorProps = Props.Create(() => new RedemptionProcessState(TestActor));
            var pmStateActor = ActorOf(stateActorProps, "state-process-manager");
            pmStateActor.Tell(
                new RedemptionController.StartOTGiftCardRedemption(
                    1234, "USD", 1000, "user@address.com"));
            pmStateActor.Tell(new SiftServiceActor.SiftScore(1234, 60));

            ExpectMsg<RedemptionController.OTGiftCardRedemptionStarted>();
            ExpectMsg<FraudCheckerActor.FraudCheckPendingManualReview>();
        }

        [Fact(DisplayName = "When the sift score is high")]
        public void When_sift_score_is_good()
        {
            var stateActorProps = Props.Create(() => new RedemptionProcessState(TestActor));
            var pmStateActor = ActorOf(stateActorProps, "state-process-manager");
            pmStateActor.Tell(
                new RedemptionController.StartOTGiftCardRedemption(
                    1234, "USD", 1000, "user@address.com"));
            pmStateActor.Tell(new SiftServiceActor.SiftScore(1234, 100));

            ExpectMsg<RedemptionController.OTGiftCardRedemptionStarted>();
            ExpectMsg<FraudCheckerActor.FraudCheckPassed>();
        }

        [Fact(DisplayName = "When the user has a good fraud score and enough points")]
        public void When_points_balance_ok()
        {
            const int gpid = 1234;
            const int requiredPoints = 1000;
            var stateActorProps = Props.Create(() => new RedemptionProcessState(TestActor));
            var pmStateActor = ActorOf(stateActorProps, "state-process-manager");
            pmStateActor.Tell(
                new RedemptionController.StartOTGiftCardRedemption(
                    gpid, "USD", requiredPoints, "user@address.com"));
            pmStateActor.Tell(new SiftServiceActor.SiftScore(gpid, 100));
            pmStateActor.Tell(new PointsService.PointsBalanceResult(gpid, requiredPoints + 1000));
            pmStateActor.Tell(new GiftService.OtGiftCardOrdered(gpid, "confirmation-number"));

            ExpectMsg<RedemptionController.OTGiftCardRedemptionStarted>();
            ExpectMsg<FraudCheckerActor.FraudCheckPassed>();
            ExpectMsg<PointsService.CheckPointsBalance>();
            ExpectMsg<GiftService.OrderOtGiftCard>();
            ExpectMsg<TmsService.NotifyUser>();
            ExpectMsg<RedemptionProcessState.GiftCardOrdered>();
        }
    }
}
