using System;

namespace LoyaltyService.FraudDetection
{
	public class SiftException : Exception
	{
		public SiftException (string message, Exception exception) : base(message, exception)
		{
		}

		public SiftException (string message) : base(message, null)
		{
		}
	}

}
