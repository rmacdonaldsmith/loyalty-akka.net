using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Xunit;

namespace LoyaltyService.Tests
{
    public class When_starting_the_redemption_process : TestKit
    {
        private ActorRef _controller;
        private long _gpid = 1234;
        private string _ccy = "USD";
        private int _pointsToRedeem = 1000;
        private string _emailAddress = "user@domain.com";

        public When_starting_the_redemption_process()
        {
            _controller = ActorOf<RedemptionController>("controller");
            _controller.Tell(new RedemptionController.StartOTGiftCardRedemption(_gpid, _ccy, _pointsToRedeem, _emailAddress));

        }

        [Fact]
        public void Should_initialze_a_new_broker_instance()
        {
            
            
        }
    }
}
