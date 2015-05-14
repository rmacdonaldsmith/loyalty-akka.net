using System;
using Akka.Actor;
using LoyaltyService.FraudDetection;


namespace LoyaltyService
{
	public class RedemptionProcessBroker : ReceiveActor
    {
        private readonly Guid _redemptionId;
	    private readonly ActorRef _redemptionStateActor;
	    private readonly ActorRef _fraudCheckActor;
	    private readonly ActorRef _siftService;
	    private readonly ActorRef _pointsService;
	    private readonly ActorRef _giftService;
	    private readonly ActorRef _notificationService;
	    private readonly ActorRef _redemptionController;

        //pass in sift, points and gift services as they are totally stateless with respect to the redeption process; they dont care
        //about the current redmptionId
	    public RedemptionProcessBroker (Guid redemptionId, ActorRef siftService, ActorRef pointsService, ActorRef giftService, ActorRef redemptionStateActor, ActorRef notificationService, ActorRef redemptionController)
	    {
	        _redemptionId = redemptionId;
	        _siftService = siftService;
	        _pointsService = pointsService;
	        _giftService = giftService;
	        _redemptionStateActor = redemptionStateActor;
	        _notificationService = notificationService;
	        _redemptionController = redemptionController;

	        Receive<RedemptionController.StartOTGiftCardRedemption>(start => 
                _redemptionStateActor.Tell(start));
            
            Become(HandleRedemptionStarted);
		}

	    public RedemptionProcessBroker()
	    {
	        //for testkit
	    }

	    private void HandleRedemptionStarted()
	    {
	        Receive<RedemptionController.OTGiftCardRedemptionStarted>(started => 
                _fraudCheckActor.Tell(
                    new FraudCheckerActor.DoFraudCheck(
                        started.Gpid, 
                        started.RedmeptionProcessId,
                        started.UserEmail,
                        started.PointsAmount,
                        new Gift(started.PointsAmount, started.Ccy)
                        )));

	        Receive<SiftServiceActor.SiftScore>(score => _fraudCheckActor.Tell(score));

            Receive<PointsService.CheckPointsBalance>(checkPoints => _pointsService.Tell(checkPoints));

	        Receive<PointsService.PointsBalanceResult>(result => _fraudCheckActor.Tell(result));

            Receive<GiftService.OrderOtGiftCard>(orderCard => _giftService.Tell(orderCard));

            Receive<GiftService.OtGiftCardOrdered>(cardOrdered => _fraudCheckActor.Tell(cardOrdered));

            Receive<TmsService.NotifyUser>(notifyUser => _notificationService.Tell(notifyUser));

            //shortcut the state actor here - seems like it maybe an unnecessary loop.
	        Receive<TmsService.UserNotified>(notified =>
	                                         _redemptionController.Tell(
	                                             new RedemptionController.RedemptionCompleted(notified.Gpid, _redemptionId)));
	    }

	    protected override SupervisorStrategy SupervisorStrategy()
        {
            return base.SupervisorStrategy();
        }
    }
}

