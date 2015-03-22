using LoyaltyService.API.HTTP.Models;
using Nancy;
using Nancy.Testing;
using NSubstitute;
using NUnit.Framework;
using RestSharp;
using System.Text.RegularExpressions;

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

		}

		[Test]
		public void ItShouldGetTheUserInformation()
		{

		}

		[Test]
		public void ItShouldGetTheUserTransactions()
		{

		}

		[Test]
		public void ItShouldSendTheRedemptionOrderToSift()
		{

		}

		[Test]
		public void ItShouldGetTheUserScoreFromSift()
		{

		}

		[Test]
		public void ItShouldSendTheGiftRequest()
		{

		}

		[Test]
		public void ItShouldCommitTheTransactionWithTheUserService(){

		}
	}
}
