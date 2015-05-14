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
        # region Messages

	    public class StartOTGiftCardRedemption : Messages.RedemptionBase
	    {
	        public StartOTGiftCardRedemption(long gpid, string ccy, int pointsAmount, string userEmail)
	            : base(gpid)
	        {
	            UserEmail = userEmail;
	            PointsAmount = pointsAmount;
	            CCY = ccy;
	        }

	        public string CCY { get; private set; }
	        public int PointsAmount { get; private set; }
	        public string UserEmail { get; private set; }
	    }

	    public class OTGiftCardRedemptionStarted : Messages.RedemptionBase
	    {
	        public Guid RedmeptionProcessId { get; private set; }

	        public OTGiftCardRedemptionStarted(long gpid, string userEmail, int pointsAmount, string ccy)
	            : base(gpid)
	        {
                UserEmail = userEmail;
                PointsAmount = pointsAmount;
                Ccy = ccy;
	        }

	        public string Ccy { get; set; }

            public int PointsAmount { get; set; }

            public string UserEmail { get; set; }
	    }

	    public class RedemptionCompleted : Messages.RedemptionBase
	    {
	        public Guid RedemptionProcessId { get; private set; }

	        public RedemptionCompleted(long gpid, Guid redmeptionId) 
                : base(gpid)
	        {
                RedemptionProcessId = redmeptionId;
	        }
	    }

	    # endregion

		private readonly Dictionary<Guid, ActorRef> _redemptions = new Dictionary<Guid, ActorRef>();
	    private ActorRef _siftService;
	    private ActorRef _pointsService;
	    private ActorRef _giftService;
	    private ActorRef _fraudChecker;
	    private ActorRef _notificationService;

	    public RedemptionController ()
		{
			Receive<StartOTGiftCardRedemption> (msg =>
			    {
			        var redemptionId = Guid.NewGuid();
                    var redemptionStateActor = Context.ActorOf(Props.Create(() => new RedemptionProcessState(Self)),
                                                    "redemption-state-actor");
			        var broker = Context.ActorOf(Props.Create(() => 
                        new RedemptionProcessBroker(redemptionId, _siftService, _pointsService, _giftService, redemptionStateActor, _notificationService, Self)), 
                        "broker-" + redemptionId.ToString());
                    _redemptions.Add(redemptionId, broker);
                    //can we use ActorSelection or something here?
			    });

	        Receive<RedemptionCompleted>(completed =>
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

	    public ReadOnlyDictionary<Guid, ActorRef> Redemptions
	    {
	        get { return new ReadOnlyDictionary<Guid, ActorRef>(_redemptions); }
	    }
	}
}

