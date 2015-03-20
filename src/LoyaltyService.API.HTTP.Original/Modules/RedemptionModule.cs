using LoyaltyService.Infrastructure;
using LoyaltyService.Messages;
using Nancy;
using Nancy.ModelBinding;
using LoyaltyService.Common;
using System;
using LoyaltyService.API.HTTP.Models;
using LoyaltyService.Messages.Exceptions;

namespace LoyaltyService.API.HTTP.Modules
{
	public class RedemptionModule : BaseModule
	{
		private readonly IBus _bus;

		public RedemptionModule (IBus bus)
		{
			_bus = bus;

			Post["/redemptions"] = parameters =>
			{
				var request = this.Bind<RedeemOtGiftCardRequest>();
				var redeemCommand = new Commands.RedeemOTGiftCard(Guid.NewGuid(), 
					request.Gpid, 
					request.Email, 
					request.NumberOfPoints, 
					request.Ccy)
				{
					SessionId = request.SessionId
				};

				try {
					_bus.Send(redeemCommand);
				} catch (RedemptionProcessException ex) {
					var response = new BaseResponse {
						Status = "not processed",
						Reason = ex.Message
					};

					return Response.AsJson(response, HttpStatusCode.BadRequest);
				} catch (NotEnoughPointsException ex) {
					var response = new BaseResponse {
						Status = "not processed",
						Reason = "not enough points"
					};

					return Response.AsJson(response, HttpStatusCode.Conflict);
				}

				return Response.AsJson(new BaseResponse { Status = "sent" }, HttpStatusCode.Created);
			};
		}
	}
}

