using System;
using System.Collections.Generic;

namespace LoyaltyService.User
{
	public class UserTransactionsResponse
	{
		public long GlobalPersonId { get; set; }
		public List<UserTransaction> Transactions { get; set; }
		public UserTransactionsResponse ()
		{
			Transactions = new List<UserTransaction> ();
		}

		public class UserTransaction
		{
			public long Id { get; set; }
			public DateTime DateTime { get; set; }
			public DateTime DateTimeUtc { get; set; }
			public string LanguageCode { get; set; }
			public string Name { get; set; }
			public int Points { get; set; }
			public TransactionType Type { get; set; }

			public int? RestaurantId { get; set; }
			public ReservationState? ReservationState { get; set; }
			public bool? IsAnonymous { get; set; }
			public bool? IsForPrimaryDiner { get; set; }
			public int? PartnerId { get; set; }
			public int? PartySize { get; set; }
		}

		public enum TransactionType {
			Reservation,
			Points
		}

		public enum ReservationState {
			Pending = 1,
			Seated = 2,
			CancelledWeb = 3,
			NoShow = 4,
			AssumedSeated = 5,
			Disputed = 6,
			SeatedDisputed = 7,
			CancelledRestaurant = 8,
			CancelledDisputed = 9,
			NoShowExcused = 10
		}
	}
}
