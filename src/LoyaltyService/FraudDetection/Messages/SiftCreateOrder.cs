using System;
using Newtonsoft.Json;

namespace LoyaltyService.FraudDetection.Messages
{
	public class SiftCreateOrder
	{
		[JsonProperty("$type")]
		public string RequestType { get; private set; }

		[JsonProperty("$api_key")]
		public string ApiKey { get; private set; }

		[JsonProperty("$user_id")]
		public string UserId { get; private set; }

		[JsonProperty("$session_id")]
		public string SessionId { get; private set; }

		[JsonProperty("$order_id")]
		public string OrderId { get; set; }

		[JsonProperty("$user_email")]
		public string UserEmail { get; set; }

		[JsonProperty("$amount")]
		public long Amount { get; set; }

		[JsonProperty("$currency_code")]
		public string CurrencyCode { get; set; }

		[JsonProperty("$shipping_address")]
		public SiftAddress Address { get; set; }

		[JsonProperty("current_points")]
		public long CurrentPoints { get; set; }

		[JsonProperty("reservations_total")]
		public long ReservationsTotal { get; set; }

		[JsonProperty("reservations_pending")]
		public long ReservationsPending { get; set; }

		[JsonProperty("reservations_seated")]
		public long ReservationsSeated { get; set; }

		[JsonProperty("reservations_assumed_seated")]
		public long ReservationsAssumedSeated { get; set; }

		[JsonProperty("reservations_disputed_seated")]
		public long ReservationsDisputedSeated { get; set; }

		[JsonProperty("reservations_no_show")]
		public long ReservationsNoShow { get; set; }

		[JsonProperty("reservations_no_show_excused")]
		public long ReservationsNoShowExcused { get; set; }

		[JsonProperty("reservations_canceled_web")]
		public long ReservationsCanceledWeb { get; set; }

		[JsonProperty("reservations_canceled_restaurant")]
		public long ReservationsCanceledRestaurant { get; set; }

		[JsonProperty("reservations_canceled_disputed")]
		public long ReservationsCanceledDisputed { get; set; }

		[JsonProperty("customer_type")]
		public string CustomerType { get; set; }

		[JsonProperty("created_date")]
		public DateTime CreateDate { get; set; }

		[JsonProperty("redemption_type")]
		public string RedemptionType { get; set; }

		[JsonProperty("redemption_points")]
		public int RedemptionPoints { get; set; }

		public SiftCreateOrder(string apiKey, string userId, string sessionId)
		{
			RequestType = "$create_order";
			ApiKey = apiKey;
			UserId = userId;
			SessionId = sessionId;
		}
	}
}
