using System;
using Akka.Actor;


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

		    Receive<Messages.Commands.StartOTGiftCardRedemption>(msg => _redemptionStateActor.Tell(msg));

	        Receive<FraudCheckerActor.FraudCheckPassed>(msg => _redemptionStateActor.Tell(msg));

            Receive<FraudCheckerActor.FraudCheckFailed>(msg => _redemptionStateActor.Tell(msg));

            Receive<FraudCheckerActor.FraudCheckPendingManualReview>(msg => _redemptionStateActor.Tell(msg));
		}

        protected override void PreStart()
        {
            _redemptionStateActor = Context.ActorOf(Props.Create<RedemptionProcessState>(_redemptionId), "RedemptionStateActor");
            _redemptionStateActor = Context.ActorOf(Props.Create(() => new RedemptionProcessState()),
                                                    "RedemptionStateActor");
        }

	    private void HandleRedemptionStarted(object message)
	    {
            //_siftService.Tell(new Messages.Commands.PerformFraudCheck(started.Gpid), Self);
            Unbecome();
	    }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return base.SupervisorStrategy();
        }
	}
}

