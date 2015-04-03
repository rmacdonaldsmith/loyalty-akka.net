namespace LoyaltyService.FraudDetection.Messages
{
	public class SiftOrderResponse : SiftBaseResponse
	{
		public long time { get; set; }
		public string request { get; set; }
	}
}
