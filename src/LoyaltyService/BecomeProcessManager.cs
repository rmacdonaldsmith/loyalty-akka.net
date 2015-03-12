using System;
using Akka.Actor;

namespace LoyaltyService
{
	public class BecomeProcessManager : ReceiveActor
	{
		private readonly ActorRef _siftGateway;
		private readonly ActorRef _pointsGateway;
		private readonly ActorRef _userServiceGateway;

		public BecomeProcessManager (ActorRef siftGateway, ActorRef pointsGateway, ActorRef userServiceGateway)
		{
			_siftGateway = siftGateway;
			_pointsGateway = pointsGateway;
			_userServiceGateway = userServiceGateway;

			Receive<Messages.RedeemOTGiftCard> (msg => {
				StartRedemptionProcess(msg);

				return true;
			});
		}

		private void StartRedemptionProcess(Messages.RedeemOTGiftCard msg)
		{
			_siftGateway.Tell (new object());
		}

		public static class Messages
		{
			public class RedeemOTGiftCard
			{
				public long GPID { get; private set; }
				public int PointsValue { get; private set; }
				public string CCY { get; private set; }

				public RedeemOTGiftCard (long gpid, int pointsValue, string ccy)
				{
					GPID = gpid;
					PointsValue = pointsValue;
					CCY = ccy;
				}
			}
		}
	}
}

