using Akka.Actor;

namespace LoyaltyService
{
    public class PointsService : ReceiveActor
    {
        # region Messages

        public class CheckPointsBalance : Messages.RedemptionBase
        {
            public CheckPointsBalance(long gpid)
                : base(gpid)
            {
            }
        }

        public class PointsBalanceResult : Messages.RedemptionBase
        {
            public int PointsBalance { get; private set; }

            public PointsBalanceResult(long gpid, int balance)
                : base(gpid)
            {
                PointsBalance = balance;
            }
        }

        # endregion

        public PointsService()
        {
            Receive<Messages.Commands.CheckPointsBalance>(msg => { });
        }
    }
}
