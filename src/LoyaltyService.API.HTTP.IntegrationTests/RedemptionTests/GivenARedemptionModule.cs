using LoyaltyService.API.HTTP.Models;
using LoyaltyService.API.HTTP.Modules;
using Nancy.Testing;
using NSubstitute;
using RestSharp;
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace LoyaltyService.API.HTTP.IntegrationTests.RedemptionTests
{
	public class GivenARedemptionModule : ServiceSpec<RedemptionModule>
	{
		protected const string SuccessFulPointsExchangeID = "PointExchangeIdFromService";

		protected BrowserResponse Redeem(RedeemOtGiftCardRequest redemptionRequest)
		{
			var response = ServiceInstance.Post("/redemptions/", with => with.JsonBody(redemptionRequest));

			return response;
		}

		protected void WithSuccessfulUserService()
		{


		}

		protected void WithSuccessfulPointsVaultService()
		{

		}

		protected void WithSuccessfulGiftService()
		{

		}

		protected void WithSuccessfulTmsService()
		{

		}

		protected void WithSuccessfulSiftService()
		{

		}
	}
}
