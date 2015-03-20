using LoyaltyService.Messages;
using Nancy;
using Nancy.Testing;
using NUnit.Framework;
using System;
using LoyaltyService.API.HTTP.Models;

namespace LoyaltyService.API.HTTP.IntegrationTests.RedemptionTests
{
	[TestFixture]
    public class WhenRedeemingAfterASuccessfulRedemption : GivenARedemptionModule
	{
		[Test]
		public void ItShouldAllowMultipleRedemptionsForAUser()
		{
			WithSuccessfulPointsVaultService ();
			WithSuccessfulUserService ();
			WithSuccessfulGiftService ();
			WithSuccessfulSiftService ();
			WithSuccessfulTmsService ();

			var previousRedemptionRequest = new RedeemOtGiftCardRequest(123, "", 200, "USD");
			var previousResponse = Redeem(previousRedemptionRequest);
			Assert.AreEqual(HttpStatusCode.Created, previousResponse.StatusCode);

			var redemptionRequest = new RedeemOtGiftCardRequest(123, "", 400, "USD");
			var response = Redeem(redemptionRequest);
			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
		}
	}
}
