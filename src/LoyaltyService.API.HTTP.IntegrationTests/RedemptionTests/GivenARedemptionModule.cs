using Nancy.Testing;
using NSubstitute;
using RestSharp;
using System;
using System.Net;
using System.Text.RegularExpressions;
using LoyaltyService.API.HTTP.Models;
using LoyaltyService.API.HTTP.Modules;
using LoyaltyService.EdgeComponents.PointsVault.Messages;
using LoyaltyService.EdgeComponents.User.Messages;
using LoyaltyService.EdgeComponents.RedeemOptions.Messages;
using LoyaltyService.EdgeComponents.FraudDetection.Messages;
using LoyaltyService.EdgeComponents.Mailer.Messages;

namespace LoyaltyService.API.HTTP.IntegrationTests.RedemptionTests
{
	public class GivenARedemptionModule : ServiceSpec<RedemptionModule>
	{
		protected const string SuccessFulPointsExchangeID = "PointExchangeIdFromService";

		protected BrowserResponse Redeem(RedeemOtGiftCardRequest redemptionRequest)
		{
			var response = ServiceInstance.Post("/redemptions/", with =>
			{
				with.JsonBody(redemptionRequest);
			});

			return response;
		}

		protected void WithSuccessfulUserService()
		{
			UserServiceClient
				.Execute<UserInfoResponse> (Arg.Is<RestRequest> (
					x => x.Method == Method.GET && new Regex("^/user/v1/users/[0-9]+(/)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).IsMatch(x.Resource)))
				.Returns (new RestResponse<UserInfoResponse> {
					Data = new UserInfoResponse {
						GlobalPersonId = 10000321,
						LoginName = "TestLogin",
						Email = "test@opentable.com",
						FirstName = "first name",
						LastName = "last name",
						Points = 100000,
						UserType = LoyaltyService.Messages.User.OTUserType.Registered,
						CreateDate = DateTime.Now,
						Address1 = "Address1",
						Address2 = "Address2",
						City = "City",
						Zip = "ZIP",
						CountryID = "US",
						PhoneNumbers = new System.Collections.Generic.List<UserInfoResponse.PhoneNumber> {
							new UserInfoResponse.PhoneNumber{
								PhoneNumberType = UserInfoResponse.PhoneNumber.OTPhoneNumberType.Mobile,
								Number = "0123456789"
							}
						}
					},
					StatusCode = HttpStatusCode.Created,
					ResponseStatus = ResponseStatus.Completed
				});

			UserServiceClient
				.Execute<UserTransactionsResponse> (Arg.Is<RestRequest> (
					x => x.Method == Method.GET && new Regex("^/user/v2/users/[0-9]+/transactions$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).IsMatch(x.Resource)))
				.Returns (new RestResponse<UserTransactionsResponse> {
					Data = new UserTransactionsResponse {
						GlobalPersonId = 10000321,
						Transactions = new System.Collections.Generic.List<UserTransactionsResponse.UserTransaction> {
							new UserTransactionsResponse.UserTransaction {
								Type = UserTransactionsResponse.TransactionType.Reservation,
								DateTimeUtc = DateTime.UtcNow,
								ReservationState = UserTransactionsResponse.ReservationState.Seated
							},
							new UserTransactionsResponse.UserTransaction {
								Type = UserTransactionsResponse.TransactionType.Points,
								DateTimeUtc = DateTime.UtcNow
							}
						}
					},
					StatusCode = HttpStatusCode.Created,
					ResponseStatus = ResponseStatus.Completed
				});

		}

		protected void WithSuccessfulPointsVaultService()
		{
			PointsVaultServiceClient
				.Execute<PointsExchangeResponses> (Arg.Is<RestRequest> (
					x => x.Method == Method.POST && x.Resource == "/points-vault/v1/exchanges"))
				.Returns (new RestResponse<PointsExchangeResponses> {
					Data = new PointsExchangeResponses {
						ResourceId = SuccessFulPointsExchangeID,
						ApiStatusCode = "SUCCESS"
					},
					StatusCode = HttpStatusCode.Created,
					ResponseStatus = ResponseStatus.Completed
				});

			PointsVaultServiceClient
				.Execute<PointsExchangeResponses> (Arg.Is<RestRequest> (
					x => x.Method == Method.PUT && x.Resource == string.Format("/points-vault/v1/exchanges/fulfill/{0}", SuccessFulPointsExchangeID)))
				.Returns (new RestResponse<PointsExchangeResponses> {
					Data = new PointsExchangeResponses {
						ApiStatusCode = "SUCCESS"
					},
					StatusCode = HttpStatusCode.OK,
					ResponseStatus = ResponseStatus.Completed
				});
		}

		protected void WithSuccessfulGiftService()
		{
			GiftServiceClient
				.Execute<GiftResponse> (Arg.Is<RestRequest> (
					x => x.Method == Method.POST && x.Resource == "/v2/gift_card_purchases/redeem_points"))
				.Returns (new RestResponse<GiftResponse> {
					Data = new GiftResponse {
						Success = true,
						Token = "foobarbaz"
					},
					StatusCode = HttpStatusCode.OK,
					ResponseStatus = ResponseStatus.Completed
			});
		}

		protected void WithSuccessfulTmsService()
		{
			TmsClient.Execute<TmsResponse> (Arg.Any<IRestRequest> ()).Returns( 
				new RestResponse<TmsResponse> { 
					Data = new TmsResponse { state = "Accepted" },
					StatusCode = HttpStatusCode.OK }); 
		}

		protected void WithSuccessfulSiftService()
		{
			SiftClient
				.Execute<SiftOrderResponse> (Arg.Is<RestRequest> (
					x => x.Method == Method.POST && x.Resource == "/v203/events"))
				.Returns (new RestResponse<SiftOrderResponse> {
					Data = new SiftOrderResponse {
						status = 0,
						error_message = "OK",
						time = 1424218795,
						request = "{\\\"$type\\\": \\\"$create_order\\\",\\\"$api_key\\\": \\\"82f6d7e6ddba0e9f\\\",\\\"$user_id\\\": \\\"testuserid\\\",\\\"$session_id\\\": \\\"restClientTest1\\\",\\\"$order_id\\\": \\\"testingorder1\\\",\\\"$user_email\\\": \\\"testemail@opentable.com\\\"}"
					},
					StatusCode = HttpStatusCode.OK,
					ResponseStatus = ResponseStatus.Completed
			});

			SiftClient
				.Execute<SiftScoreResponse> (Arg.Is<RestRequest> (
					x => x.Method == Method.GET && x.Resource.StartsWith ("/v203/score/")))
				.Returns (new RestResponse<SiftScoreResponse> {
					Data = new SiftScoreResponse {
						status = 0,
						error_message = "OK",
						score = 0.01f
					},
					StatusCode = HttpStatusCode.OK,
					ResponseStatus = ResponseStatus.Completed
			});
		}
	}
}
