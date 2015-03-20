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
		private readonly Dictionary<Guid, ActorRef> _redemptions = new Dictionary<Guid, ActorRef>();
	    private ActorRef _siftService;
	    private ActorRef _pointsService;
	    private ActorRef _giftService;

		public RedemptionController ()
		{
			Receive<Messages.Commands.StartOTGiftCardRedemption> (msg =>
			    {
			        var redemptionId = Guid.NewGuid();
			        var reference = Context.ActorOf(Props.Create(() => 
                        new RedemptionProcessManager(redemptionId, _siftService, _pointsService, _giftService)), 
                        "pm-" + redemptionId.ToString());
                    _redemptions.Add(redemptionId, reference); //we probably dont have to keep a list of actorrefs like this
                    //can we use ActorSelection or something here?
			    });
		}

        protected override void PreStart()
        {
            _siftService = Context.ActorOf(Props.Create<SiftService>());
            _pointsService = Context.ActorOf(Props.Create<PointsService>());
            _giftService = Context.ActorOf(Props.Create<GiftService>());
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            //probably want to just restart the children if they fail
            return base.SupervisorStrategy();
        }
	}
}

