using System;
using Akka.Actor;
using LoyaltyService.FraudDetection;


namespace LoyaltyService
{
	public class RedemptionProcessBroker : ReceiveActor
    {
        private readonly Guid _redemptionId;
	    private ActorRef _redemptionStateActor;
	    private readonly ActorRef _siftService;
	    private readonly ActorRef _pointsService;
	    private readonly ActorRef _giftService;

        //pass in sift, points and gift services as they are totally stateless with respect to the redeption process; they dont care
        //about the current redmptionId
	    public RedemptionProcessBroker (Guid redemptionId, ActorRef siftService, ActorRef pointsService, ActorRef giftService)
	    {
	        _redemptionId = redemptionId;
	        _siftService = siftService;
	        _pointsService = pointsService;
	        _giftService = giftService;

		    Receive<RedemptionController.StartOTGiftCardRedemption>(msg => _redemptionStateActor.Tell(msg));

            Become(HandleRedemptionStarted);
		}

	    public RedemptionProcessBroker()
	    {
	        //for testkit
	    }

	    protected override void PreStart()
        {
            _redemptionStateActor = Context.ActorOf(Props.Create(() => new RedemptionProcessState(Self)),
                                                    "RedemptionStateActor");
        }

	    private void HandleRedemptionStarted(object message)
	    {
            Unbecome();
	    }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return base.SupervisorStrategy();
        }
    }
}

