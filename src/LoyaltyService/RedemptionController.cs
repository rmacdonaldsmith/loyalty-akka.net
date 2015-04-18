using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Akka.Actor;
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

	        public OTGiftCardRedemptionStarted(long gpid)
	            : base(gpid)
	        {

	        }
	    }

	    # endregion

		private readonly Dictionary<Guid, ActorRef> _redemptions = new Dictionary<Guid, ActorRef>();
	    private ActorRef _siftService;
	    private ActorRef _pointsService;
	    private ActorRef _giftService;
	    private ActorRef _fraudChecker;

	    public RedemptionController ()
		{
			Receive<StartOTGiftCardRedemption> (msg =>
			    {
			        var redemptionId = Guid.NewGuid();
			        var broker = Context.ActorOf(Props.Create(() => 
                        new RedemptionProcessBroker(redemptionId, _siftService, _pointsService, _giftService)), 
                        "pm-" + redemptionId.ToString());
                    _redemptions.Add(redemptionId, broker); //we probably dont have to keep a list of actorrefs like this
                    //can we use ActorSelection or something here?
			    });
		}

        protected override void PreStart()
        {
            _fraudChecker = Context.ActorOf(Props.Create<FraudCheckerActor>());
            _siftService = Context.ActorOf(Props.Create<SiftServiceActor>());
            _pointsService = Context.ActorOf(Props.Create<PointsService>());
            _giftService = Context.ActorOf(Props.Create<GiftService>());
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            //probably want to just restart the children if they fail
            return base.SupervisorStrategy();
        }

	    public ReadOnlyDictionary<Guid, ActorRef> Redemptions
	    {
	        get { return new ReadOnlyDictionary<Guid,ActorRef>(_redemptions); }
	    }
	}
}

