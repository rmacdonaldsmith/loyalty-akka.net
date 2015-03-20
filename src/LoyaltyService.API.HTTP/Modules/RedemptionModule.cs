using Nancy;
using Nancy.ModelBinding;
using System;
using LoyaltyService.API.HTTP.Models;

namespace LoyaltyService.API.HTTP.Modules
{
	public class RedemptionModule : BaseModule
	{

		public RedemptionModule ()
		{
			Post["/redemptions"] = parameters =>
			{
				var request = this.Bind<RedeemOtGiftCardRequest>();
                return Response.AsJson(Guid.NewGuid(), HttpStatusCode.Conflict);

			};
		}
	}
}

