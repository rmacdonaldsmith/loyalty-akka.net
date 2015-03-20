using System;

namespace LoyaltyService.Common
{
	public class RedemptionProcessException : Exception
	{
		public Guid RedemptionId { get; set; }
		public long GPID { get; set; }

		public RedemptionProcessException (Guid redemptionId, long gpid, string message)
			: base(message)
		{
			RedemptionId = redemptionId;
			GPID = gpid;
		}
	}
}

