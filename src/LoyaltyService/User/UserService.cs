using System;
using System.Collections.Generic;
using System.Linq;
using LoyaltyService.FraudDetection.Messages;
using RestSharp;

namespace LoyaltyService.User
{
	public class UserService
	{
		private readonly IRestClient _restClient;

		public UserService(IRestClient restClient)
		{
			_restClient = restClient;
		}

		public UserInfo GetUserInfo(long gpid)
		{
			var request = new RestRequest (string.Format ("/user/v1/users/{0}", gpid), Method.GET);

			IRestResponse<UserInfoResponse> response = null;
			try
			{
				response = _restClient.Get<UserInfoResponse> (request);
			}
			catch (Exception exception)
			{
				throw new Exception ("could not call the user info service", exception);
			}

			var userResponse = response.Data;
			var userPhoneNumber = GetUserMainPhoneNumber (userResponse.PhoneNumbers);
			var user = new UserInfo {
				GPID = userResponse.GlobalPersonId,
				LoginName = userResponse.LoginName,
				Email = userResponse.Email,
				FirstName = userResponse.FirstName,
				LastName = userResponse.LastName,
				Points = userResponse.Points,
				UserType = userResponse.UserType,
				CreateDate = userResponse.CreateDate,
				Address1 = userResponse.Address1,
				Address2 = userResponse.Address2,
				City = userResponse.City,
				Zip = userResponse.Zip,
				CountryID = userResponse.CountryID,
				PhoneNumber = userPhoneNumber
			};

			return user;
		}

		public ReservationsSummary GetReservationsSummary (long gpid)
		{
			var request = new RestRequest (string.Format ("/user/v2/users/{0}/transactions", gpid), Method.GET);

			IRestResponse<UserTransactionsResponse> response = null;
			try
			{
				response = _restClient.Get<UserTransactionsResponse> (request);
			}
			catch (Exception exception)
			{
				throw new Exception ("could not call the user transaction service", exception);
			}

			var transactions = response.Data.Transactions;
			var reservations = transactions.Where (t => t.Type == UserTransactionsResponse.TransactionType.Reservation).ToList();
			var summary = new ReservationsSummary {
				NumberOfReservations = reservations.Count,
				NumberOfReservationsInLastMonth = reservations
					.LongCount (r => r.DateTimeUtc.Date >= DateTime.UtcNow.AddMonths (-1).Date),
				NumberOfPendingReservations = reservations
					.LongCount (r => r.ReservationState.Value == UserTransactionsResponse.ReservationState.Pending),
				NumberOfSeatedReservations = reservations
					.LongCount (r => r.ReservationState.Value == UserTransactionsResponse.ReservationState.Seated),
				NumberOfAssumedSeatedReservations = reservations
					.LongCount (r => r.ReservationState.Value == UserTransactionsResponse.ReservationState.AssumedSeated),
				NumberOfDisputedSeatedReservations = reservations
					.LongCount (r => r.ReservationState.Value == UserTransactionsResponse.ReservationState.SeatedDisputed),
				NumberOfNoShowReservations = reservations
					.LongCount (r => r.ReservationState.Value == UserTransactionsResponse.ReservationState.NoShow),
				NumberOfNoShowExcusedReservations = reservations
					.LongCount (r => r.ReservationState.Value == UserTransactionsResponse.ReservationState.NoShowExcused),
				NumberOfCanceledWebReservations = reservations
					.LongCount (r => r.ReservationState.Value == UserTransactionsResponse.ReservationState.CancelledWeb),
				NumberOfCanceledRestaurantReservations = reservations
					.LongCount (r => r.ReservationState.Value == UserTransactionsResponse.ReservationState.CancelledRestaurant),
				NumberOfCanceledDisputedReservations = reservations
					.LongCount (r => r.ReservationState.Value == UserTransactionsResponse.ReservationState.CancelledDisputed)
			};
			return summary;
		}

		private string GetUserMainPhoneNumber (List<UserInfoResponse.PhoneNumber> phoneNumbers)
		{
			if (phoneNumbers.Count < 1) {
				return string.Empty;
			}

			var mobilePhone = phoneNumbers.Find (p => p.PhoneNumberType == UserInfoResponse.PhoneNumber.OTPhoneNumberType.Mobile);
			var otherPhone = phoneNumbers.Find (p => p.PhoneNumberType != UserInfoResponse.PhoneNumber.OTPhoneNumberType.Mobile);

			if (mobilePhone != null) {
				return mobilePhone.Number;
			} else if (otherPhone != null) {
				return otherPhone.Number;
			} else {
				return string.Empty;
			}
		}
	}
}
