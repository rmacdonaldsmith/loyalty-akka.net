using System;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Xunit;

namespace LoyaltyService.Tests
{
    public class When_starting_the_redemption_process : TestKit
    {
        private IActorRef _controller;
        private long _gpid = 1234;
        private Guid _redemptionId = Guid.NewGuid();
        private string _ccy = "USD";
        private int _pointsToRedeem = 1000;
        private string _emailAddress = "user@domain.com";

        public When_starting_the_redemption_process()
        {
            _controller = ActorOf<RedemptionController>("controller");
            _controller.Tell(new Commands.StartOTGiftCardRedemption(_gpid, _redemptionId, _ccy, _pointsToRedeem, _emailAddress));

        }

        [Fact]
        public void Should_initialze_a_new_broker_instance()
        {
            
            
        }
    }
}
