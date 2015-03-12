using System;
using Akka.Actor;


namespace LoyaltyService
{
	public enum States
	{
		StartingRedemption,
		PerformingFraudCheck,
		CheckingPointsBalance,
		OrderingGiftCard,
		GiftCardOrdered
	}

	public class Data
	{
		public Guid RedemptionId { get; set; }
		public States CurrentState { get; set; }
		public long GPID { get; set; }

		public Data (long gpid)
		{
			RedemptionId = Guid.NewGuid ();
			GPID = gpid;
		}
	}

	public class RedemptionProcessManager : FSM<States, Data>
	{
		public RedemptionProcessManager ()
		{
			StartWith(States.StartingRedemption, new Data(0));

//			When (States.PerformingFraudCheck, (Event<Data> fsmEvent) => {
//			                                                                 return new State<States, Data>();
//			});


		}
	}
}

