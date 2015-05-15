using Akka.Actor;

namespace LoyaltyService
{
    public class PointsService : ReceiveActor
    {
        public PointsService()
        {
            Receive<Commands.CheckPointsBalance>(
                msg =>
                Context.ActorSelection("/*/broker")
                       .Tell(new Events.PointsBalanceResult(msg.Gpid, msg.RedemptionId, 2000)));
        }
    }
}
