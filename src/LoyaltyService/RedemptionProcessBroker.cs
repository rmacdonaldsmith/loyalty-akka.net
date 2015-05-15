using System;
using Akka.Actor;
using LoyaltyService.FraudDetection;


namespace LoyaltyService
{
	public class RedemptionProcessBroker : ReceiveActor
    {
        private readonly Guid _redemptionId;
	    private readonly IActorRef _redemptionStateActor;
	    private readonly IActorRef _fraudCheckActor;
	    private readonly IActorRef _siftService;
	    private readonly IActorRef _pointsService;
	    private readonly IActorRef _giftService;
	    private readonly IActorRef _notificationService;
	    private readonly IActorRef _redemptionController;

        //pass in sift, points and gift services as they are totally stateless with respect to the redeption process; they dont care
        //about the current redmptionId
	    public RedemptionProcessBroker (Guid redemptionId, IActorRef siftService, IActorRef pointsService, IActorRef giftService, IActorRef redemptionStateActor, IActorRef notificationService, IActorRef redemptionController)
	    {
	        _redemptionId = redemptionId;
	        _siftService = siftService;
	        _pointsService = pointsService;
	        _giftService = giftService;
	        _redemptionStateActor = redemptionStateActor;
	        _notificationService = notificationService;
	        _redemptionController = redemptionController;

	        Receive<Commands.StartOTGiftCardRedemption>(start => 
                _redemptionStateActor.Tell(start));
            
            Become(HandleRedemptionStarted);
		}

	    public RedemptionProcessBroker()
	    {
	        //for testkit
	    }

	    private void HandleRedemptionStarted()
	    {
	        Receive<Events.OTGiftCardRedemptionStarted>(started => 
                _fraudCheckActor.Tell(
                    new Commands.DoFraudCheck(
                        started.Gpid, 
                        started.RedemptionId,
                        started.UserEmail,
                        started.PointsAmount,
                        new Gift(started.PointsAmount, started.Ccy)
                        )));

	        Receive<Events.SiftScore>(score => _fraudCheckActor.Tell(score));

            Receive<Commands.CheckPointsBalance>(checkPoints => _pointsService.Tell(checkPoints));

	        Receive<Events.PointsBalanceResult>(result => _fraudCheckActor.Tell(result));

            Receive<Commands.OrderOtGiftCard>(orderCard => _giftService.Tell(orderCard));

            Receive<Events.OtGiftCardOrdered>(cardOrdered => _fraudCheckActor.Tell(cardOrdered));

            Receive<Commands.NotifyUser>(notifyUser => _notificationService.Tell(notifyUser));

            //shortcut the state actor here - seems like it maybe an unnecessary loop.
	        Receive<Events.UserNotified>(notified =>
	                                         _redemptionController.Tell(
	                                             new Events.RedemptionCompleted(notified.Gpid, _redemptionId)));
	    }

	    protected override SupervisorStrategy SupervisorStrategy()
        {
            return base.SupervisorStrategy();
        }
    }
}

