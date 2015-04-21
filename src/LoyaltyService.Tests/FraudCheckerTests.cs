using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit;
using LoyaltyService.FraudDetection;
using LoyaltyService.User;
using RestSharp;
using Xunit;

namespace LoyaltyService.Tests
{
    public class FraudCheckerTests : TestKit
    {
        private readonly ActorRef _fraudChecker;
        private readonly IRestClient _userRestClient;
        private readonly IRestClient _siftRestClient;
        private readonly UserService _userService;
        private readonly SiftService _siftService;
        private readonly Guid _redemptionId = Guid.NewGuid();
        private const long Gpid = 1234;
        private const string Ccy = "USD";
        private const int PointsToRedeem = 1000;
        private const string EmailAddress = "user@domain.com";

        public FraudCheckerTests(IRestClient userRestClient, IRestClient siftRestClient)
        {
            _userRestClient = userRestClient;
            _siftRestClient = siftRestClient;
            _userService = new UserService(_userRestClient);
            _siftService = new SiftService(_siftRestClient, "fake-api-key");
            var userServiceActorProps = Props.Create(() => new UserServiceActor(_fraudChecker, _userService));
            var siftServiceActorProps = Props.Create(() => new SiftServiceActor(_fraudChecker, _siftService));
            var fraudCheckProps =
                Props.Create(() => new FraudCheckerActor(TestActor, userServiceActorProps, siftServiceActorProps));
            _fraudChecker = ActorOf(fraudCheckProps, "fraud-checker");
        }

        [Fact]
        public void When_performing_the_fraud_check()
        {
            _fraudChecker.Tell(new FraudCheckerActor.DoFraudCheck(Gpid, _redemptionId, EmailAddress, PointsToRedeem, new Gift(PointsToRedeem, Ccy)));

            ExpectMsg<SiftServiceActor.FraudCheckPassed>();
        }
    }
}
