﻿using System;
using Akka.Actor;


namespace LoyaltyService
{
	public class RedemptionProcessManager : ReceiveActor
	{
	    private readonly Guid _redemptionId;
	    private readonly ActorRef _redemptionStateActor;
	    private readonly ActorRef _siftService;
	    private readonly ActorRef _pointsService;
	    private readonly ActorRef _giftService;

        //pass in sift, points and gift services as they are totally stateless with respect to the redeption process; they dont care
        //about the current redmptionId
	    public RedemptionProcessManager (Guid redemptionId, ActorRef siftService, ActorRef pointsService, ActorRef giftService)
	    {
	        _redemptionId = redemptionId;
	        _siftService = siftService;
	        _pointsService = pointsService;
	        _giftService = giftService;
	        _redemptionStateActor = Context.ActorOf(Props.Create<RedemptionProcessState>(_redemptionId),"RedemptionStateActor");

		    Receive<Messages.Commands.StartOTGiftCardRedemption>(msg =>
		        {
                    _redemptionStateActor.Tell(msg, Self); //todo: is Self implicitly sent with Tell(), do I need to specify Self here?
                    _siftService.Tell(new Messages.Commands.DoFraudCheck(msg.Gpid), Self);
		            return true;
		        });

	        Receive<Messages.Events.OTGiftCardRedemptionStarted>(msg =>
	            {
	                //do i need to Become in here so that i can start to coordinate other message interactions?
	            });

	        Receive<Messages.Events.FraudCheckPassed>(msg => _redemptionStateActor.Tell(msg));

	        Receive<Messages.Events.FraudCheckFailed>(msg => _redemptionStateActor.Tell(msg));

	        Receive<Messages.Events.FraudCheckPendingManualReview>(msg => _redemptionStateActor.Tell(msg));
		}
	}
}

