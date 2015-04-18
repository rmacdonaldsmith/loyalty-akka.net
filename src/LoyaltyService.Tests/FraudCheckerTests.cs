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
        private ActorRef _fraudChecker;
        private IRestClient _userRestClient;
        private IRestClient _siftRestClient;
        private UserService _userService;
        private SiftService _siftService;
        private Guid _redemptionId = Guid.NewGuid();
        private long _gpid = 1234;
        private string _ccy = "USD";
        private int _pointsToRedeem = 1000;
        private string _emailAddress = "user@domain.com";

        public FraudCheckerTests()
        {
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
            _fraudChecker.Tell(new FraudCheckerActor.DoFraudCheck(_gpid, _redemptionId, _emailAddress, _pointsToRedeem, new Gift(_pointsToRedeem, _ccy)));

            ExpectMsg<SiftServiceActor.FraudCheckPassed>();
        }
    }
}
