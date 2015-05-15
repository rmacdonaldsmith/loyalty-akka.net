using Akka.Actor;
using LoyaltyService.FraudDetection;
using Xunit;

namespace LoyaltyService.Tests
{
    public class FraudCheckFails : GivenAFraudCheckerActor
    {
        public FraudCheckFails()
        {
            WithDefaultUserRegistrations();
            WithSuccessfulSiftScoreResponse(0.1f);
            WithSuccessfulUserInfoResponse();
            WithSuccessfullSiftOrder();
            InitializeFraudChecker();
        }

        [Fact(DisplayName = "When the fraud check fails")]
        public void When_performing_the_fraud_check()
        {
            FraudChecker.Tell(new Commands.DoFraudCheck(Gpid, RedemptionId, EmailAddress, PointsToRedeem, new Gift(PointsToRedeem, Ccy)));

            ExpectMsg<Events.SiftScore>(score => score.Score == 10);
        }
    }
}
