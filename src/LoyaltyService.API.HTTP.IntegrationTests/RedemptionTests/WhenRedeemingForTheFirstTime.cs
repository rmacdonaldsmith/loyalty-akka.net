using Nancy;
using Nancy.Testing;
using NSubstitute;
using NUnit.Framework;
using RestSharp;
using System.Text.RegularExpressions;
using LoyaltyService.EdgeComponents.PointsVault.Messages;
using LoyaltyService.EdgeComponents.User.Messages;
using LoyaltyService.EdgeComponents.RedeemOptions.Messages;
using LoyaltyService.EdgeComponents.FraudDetection.Messages;
using LoyaltyService.API.HTTP.Models;

namespace LoyaltyService.API.HTTP.IntegrationTests.RedemptionTests
{
	[TestFixture]
	public class WhenRedeemingOnce : GivenARedemptionModule
	{
		BrowserResponse _response;

		[TestFixtureSetUp]
		public void Setup()
		{
			PointsVaultServiceClient.ClearReceivedCalls ();
			WithSuccessfulPointsVaultService ();

			UserServiceClient.ClearReceivedCalls ();
			WithSuccessfulUserService ();

			GiftServiceClient.ClearReceivedCalls ();
			WithSuccessfulGiftService ();

			SiftClient.ClearReceivedCalls ();
			WithSuccessfulSiftService ();

			TmsClient.ClearReceivedCalls ();
			WithSuccessfulTmsService ();

			var redemptionRequest = new RedeemOtGiftCardRequest(1234567890, "test@email.com", 200, "USD");
			_response = Redeem(redemptionRequest); ;
		}

		[Test]
		public void ItShouldCreateARedemptionMessage()
		{
			Assert.AreEqual(HttpStatusCode.Created, _response.StatusCode);
		}

		[Test]
		public void ItShouldStartTheTransactionWithTheUserService()
		{
			PointsVaultServiceClient.Received(1).Execute<PointsExchangeResponses>(Arg.Is<RestRequest>(x =>
				x.Method == Method.POST && x.Resource == "/points-vault/v1/exchanges"
			));
		}

		[Test]
		public void ItShouldGetTheUserInformation()
		{
			UserServiceClient.Received(1).Execute<UserInfoResponse>(Arg.Is<RestRequest>(x =>
				x.Method == Method.GET && new Regex("^/user/v1/users/[0-9]+(/)?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).IsMatch(x.Resource)
			));
		}

		[Test]
		public void ItShouldGetTheUserTransactions()
		{
			UserServiceClient.Received(1).Execute<UserTransactionsResponse>(Arg.Is<RestRequest>(x =>
				x.Method == Method.GET && new Regex("^/user/v2/users/[0-9]+/transactions$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).IsMatch(x.Resource)
			));
		}

		[Test]
		public void ItShouldSendTheRedemptionOrderToSift()
		{
			SiftClient.Received (1).Execute<SiftOrderResponse> (Arg.Is<RestRequest> (x =>
				x.Method == Method.POST && x.Resource == "/v203/events"
			));
		}

		[Test]
		public void ItShouldGetTheUserScoreFromSift()
		{
			SiftClient.Received (1).Execute<SiftScoreResponse> (Arg.Is<RestRequest> (x =>
				x.Method == Method.GET && x.Resource.StartsWith ("/v203/score/")
			));
		}

		[Test]
		public void ItShouldSendTheGiftRequest()
		{
			GiftServiceClient.Received (1).Execute<GiftResponse> (Arg.Is<RestRequest>( x =>
				x.Method == Method.POST && x.Resource == "/v2/gift_card_purchases/redeem_points"
			));
		}

		[Test]
		public void ItShouldCommitTheTransactionWithTheUserService(){
			PointsVaultServiceClient.Received (1).Execute<PointsExchangeResponses>(Arg.Is<RestRequest> (x =>
				x.Method == Method.PUT && x.Resource == string.Format("/points-vault/v1/exchanges/fulfill/{0}", SuccessFulPointsExchangeID)
			));
		}
	}
}
