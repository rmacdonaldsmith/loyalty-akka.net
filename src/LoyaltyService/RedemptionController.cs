using System;
using System.Collections.Generic;
using Akka.Actor;

namespace LoyaltyService
{
	/// <summary>
	/// Redemption controller keeps track of all redemptions currently underway. Each redemption is managed
	/// by an instance of a ProcessRedemptionManager. The RedemptionController actor will spawn a new 
	/// process manager for each new redemption request that comes in.
	/// </summary>
	public class RedemptionController : ReceiveActor
	{
		private readonly List<ActorRef> _redemptions = new List<ActorRef>();

		public RedemptionController ()
		{
			Receive<Messages.Commands.StartOTGiftCardRedemption> (msg =>
			    {
			        var reference = Context.ActorOf<RedemptionController>(
			            Props.Create(typeof (RedemptionController)).ToString());
                    _redemptions.Add(reference);
			    });
		}
	}
}

