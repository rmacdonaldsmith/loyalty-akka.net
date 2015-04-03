using Newtonsoft.Json;

namespace LoyaltyService.FraudDetection.Messages
{
	public class SiftAddress
	{
		[JsonProperty("$name")]
		public string Name { get; set; }

		[JsonProperty("$phone")]
		public string Phone { get; set; }

		[JsonProperty("$address_1")]
		public string Address1 { get; set; }

		[JsonProperty("$address_2")]
		public string Address2 { get; set; }

		[JsonProperty("$city")]
		public string City { get; set; }

		[JsonProperty("$region")]
		public string Region { get; set; }

		[JsonProperty("$country")]
		public string Country { get; set; }

		[JsonProperty("$zipcode")]
		public string ZipCode { get; set; }
	}
}
