using System;
using System.IO;
using LoyaltyService.FraudDetection.Messages;
using Newtonsoft.Json;
using RestSharp;
using System.Net;
using RestSharp.Serializers;

namespace LoyaltyService.FraudDetection
{
	public class SiftService
	{
		private readonly IRestClient _restClient;
		private readonly string _apiKey;

		public SiftService (IRestClient restClient, string siftApiKey)
		{
			_restClient = restClient;
		    _apiKey = siftApiKey;
		}

		public void SendOrderInformation(UserInfo userInfo, ReservationsSummary reservations, SiftServiceActor.CheckRequestForFraud requestOrder)
		{
			var createOrderRequest = new SiftCreateOrder (_apiKey, requestOrder.GPID.ToString(), requestOrder.SessionId){
				OrderId = requestOrder.RedemptionId.ToString(),
				UserEmail = userInfo.Email,
				RedemptionType = "OTGift",
				RedemptionPoints = requestOrder.PointsToRedeem,
				Amount = requestOrder.Gift.Amount * 1000000,
				CurrencyCode = requestOrder.Gift.CCY,
				Address = new SiftAddress {
					Address1 = userInfo.Address1,
					Address2 = userInfo.Address2,
					City = userInfo.City,
					ZipCode = userInfo.Zip,
					Country = userInfo.CountryID,
					Name = string.Format("{0} {1}", userInfo.FirstName, userInfo.LastName),
					Phone = userInfo.PhoneNumber
				},
				CustomerType = userInfo.UserType.ToString(),
				CreateDate = userInfo.CreateDate,
				CurrentPoints = userInfo.Points,
				ReservationsTotal = reservations.NumberOfReservations,
				ReservationsPending = reservations.NumberOfPendingReservations,
				ReservationsSeated = reservations.NumberOfSeatedReservations,
				ReservationsAssumedSeated = reservations.NumberOfAssumedSeatedReservations,
				ReservationsDisputedSeated = reservations.NumberOfDisputedSeatedReservations,
				ReservationsNoShow = reservations.NumberOfNoShowReservations,
				ReservationsNoShowExcused = reservations.NumberOfNoShowExcusedReservations,
				ReservationsCanceledWeb = reservations.NumberOfCanceledWebReservations,
				ReservationsCanceledRestaurant = reservations.NumberOfCanceledRestaurantReservations,
				ReservationsCanceledDisputed = reservations.NumberOfCanceledDisputedReservations
			};

			var request = new RestRequest ("/v203/events", Method.POST) {
					RequestFormat = DataFormat.Json,
					JsonSerializer = new RestSharpJsonSerializer ()
				}
				.AddBody (createOrderRequest);

			IRestResponse<SiftOrderResponse> response = null;
			response = _restClient.Post<SiftOrderResponse> (request);

			if (!ResponseIsValid(response)) {
				var exception = new SiftException ("Could not send the order to sift");
				exception.Data.Add ("userInfo", userInfo);
				exception.Data.Add ("reservations", reservations);
				exception.Data.Add ("requestOrder", requestOrder);
				AddRestSharpDataToException (response, exception);

				throw exception;
			}
		}

		public int GetUserScore(long gpid)
		{
			var request = new RestRequest (string.Format ("/v203/score/{0}/", gpid), Method.GET)
				.AddQueryParameter ("api_key", _apiKey);

			IRestResponse<SiftScoreResponse> response = null;
			response = _restClient.Get<SiftScoreResponse> (request);

			if (!ResponseIsValid(response)) {
				var exception = new SiftException ("Could not get the score from sift");
				exception.Data.Add ("gpid", gpid);
				AddRestSharpDataToException (response, exception);
				throw exception;
			}

			return (int) Math.Round(response.Data.score * 100);
		}

		private static bool ResponseIsValid<T>(IRestResponse<T> response) where T : SiftBaseResponse
		{
			return response != null
				&& response.ResponseStatus == ResponseStatus.Completed
				&& response.StatusCode == HttpStatusCode.OK
				&& response.Data != null
				&& response.Data.status == 0
				&& response.Data.error_message == "OK";
		}

		private static void AddRestSharpDataToException<T> (IRestResponse<T> response, Exception exception)
		{
			exception.Data.Add ("restsharp.responseStatus", response.ResponseStatus);
			exception.Data.Add ("restsharp.statusCode", response.StatusCode);
			exception.Data.Add ("restsharp.responseUri", response.ResponseUri);
			exception.Data.Add ("restsharp.content", response.Content);
			exception.Data.Add ("restsharp.errorMessage", response.ErrorMessage);
			exception.Data.Add ("restsharp.errorException", response.ErrorException);
		}

        public class RestSharpJsonSerializer : ISerializer
        {
            private readonly Newtonsoft.Json.JsonSerializer _serializer;

            public RestSharpJsonSerializer()
            {
                ContentType = "application/json";
                _serializer = new Newtonsoft.Json.JsonSerializer
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Include
                };
            }

            public string Serialize(object obj)
            {
                using (var stringWriter = new StringWriter())
                {
                    using (var jsonWriter = new JsonTextWriter(stringWriter))
                    {
                        jsonWriter.Formatting = Formatting.Indented;
                        jsonWriter.QuoteChar = '"';

                        _serializer.Serialize(jsonWriter, obj);
                        var result = stringWriter.ToString();
                        return result;
                    }
                }
            }

            public string RootElement { get; set; }

            public string Namespace { get; set; }

            public string DateFormat { get; set; }

            public string ContentType { get; set; }
        }
	}
}
