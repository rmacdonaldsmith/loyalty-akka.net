using LoyaltyService.FraudDetection;
using Xunit;

namespace LoyaltyService.Tests
{
    public class FraudCheckSucceeds : GivenAFraudCheckerActor
    {
        public FraudCheckSucceeds()
        {
            WithDefaultUserRegistrations();
            WithSuccessfulSiftScoreResponse();
            WithSuccessfulUserInfoResponse();
            WithSuccessfullSiftOrder();
            InitializeFraudChecker();
        }

        [Fact(DisplayName = "When the fraud check is successful")]
        public void When_performing_the_fraud_check()
        {
            FraudChecker.Tell(new FraudCheckerActor.DoFraudCheck(Gpid, RedemptionId, EmailAddress, PointsToRedeem, new Gift(PointsToRedeem, Ccy)));
            
            ExpectMsg<SiftServiceActor.SiftScore>(score => score.Score == 100);
        }
    }
}
