using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Akka.Actor;
using Akka.Event;
using LoyaltyService.FraudDetection;

namespace LoyaltyService
{
	/// <summary>
	/// Redemption controller keeps track of all redemptions currently underway. Each redemption is managed
	/// by an instance of a ProcessRedemptionManager. The RedemptionController actor will spawn a new 
	/// process manager for each new redemption request that comes in.
	/// </summary>
	public class RedemptionController : ReceiveActor
	{
		private readonly Dictionary<Guid, IActorRef> _redemptions = new Dictionary<Guid, IActorRef>();
	    private IActorRef _siftService;
	    private IActorRef _pointsService;
	    private IActorRef _giftService;
	    private IActorRef _fraudChecker;
	    private IActorRef _notificationService;

	    public RedemptionController ()
		{
			Receive<Commands.StartOTGiftCardRedemption> (msg =>
			    {
			        var redemptionId = Guid.NewGuid();
                    var redemptionStateActor = Context.ActorOf(Props.Create(() => new RedemptionProcessStateActor(Self)),
                                                    "redemption-state-actor");
			        var broker = Context.ActorOf(Props.Create(() => 
                        new RedemptionProcessBroker(redemptionId, _siftService, _pointsService, _giftService, redemptionStateActor, _notificationService, Self)), 
                        "broker-" + redemptionId.ToString());
                    _redemptions.Add(redemptionId, broker);
                    //can we use ActorSelection or something here?
			    });

	        Receive<Events.RedemptionCompleted>(completed =>
	            {
	                if (_redemptions.ContainsKey(completed.RedemptionProcessId))
	                    _redemptions.Remove(completed.RedemptionProcessId);
	                else
	                    Context.GetLogger()
	                           .Error(
	                               "Received a RedemptionCompleted event for redmptionId [{0}] but no redemption exists with that id.",
	                               completed.RedemptionProcessId);
	            });
		}

        protected override void PreStart()
        {
            _fraudChecker = Context.ActorOf(Props.Create<FraudCheckerActor>());
            _siftService = Context.ActorOf(Props.Create<SiftServiceActor>());
            _pointsService = Context.ActorOf(Props.Create<PointsService>());
            _giftService = Context.ActorOf(Props.Create<GiftService>());
            _notificationService = Context.ActorOf(Props.Create<TmsService>());
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            //probably want to just restart the children if they fail
            return base.SupervisorStrategy();
        }

	    public ReadOnlyDictionary<Guid, IActorRef> Redemptions
	    {
	        get { return new ReadOnlyDictionary<Guid, IActorRef>(_redemptions); }
	    }
	}
}

