using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

namespace LoyaltyService
{
    public class PointsService : ReceiveActor
    {
        public PointsService()
        {
            Receive<Messages.Commands.CheckPointsBalance>(msg => { });
        }
    }
}
