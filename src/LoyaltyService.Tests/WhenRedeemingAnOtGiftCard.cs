using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit;
using Xunit;
using Akka.TestKit;

namespace LoyaltyService.Tests
{
    public class WhenRedeemingAnOtGiftCard : TestKit
    {
        [Fact]
        public void Initialize_with_first_message()
        {
            var broker = ActorOf<RedemptionProcessBroker>("broker");
        }
    }
}
