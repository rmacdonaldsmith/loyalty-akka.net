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
            FraudChecker.Tell(new FraudCheckerActor.DoFraudCheck(Gpid, RedemptionId, EmailAddress, PointsToRedeem, new Gift(PointsToRedeem, Ccy)));

            ExpectMsg<SiftServiceActor.SiftScore>(score => score.Score == 10);
        }
    }
}
