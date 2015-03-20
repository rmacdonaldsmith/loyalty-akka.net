using System;
using Newtonsoft.Json;

namespace LoyaltyService.API.HTTP.Models
{
	public class RedeemOtGiftCardRequest
	{
		public RedeemOtGiftCardRequest() { }

		public RedeemOtGiftCardRequest(long gpid, string email, int numberOfPoints, string ccy)
		{
			Gpid = gpid;
			NumberOfPoints = numberOfPoints;
			Ccy = ccy;
			Email = email;
		}

		public long Gpid { get; set; }
		public string Email { get; set; }
		public int NumberOfPoints { get; set; }
		public string Ccy { get; set; }
		public string SessionId { get; set; }
	}
}

