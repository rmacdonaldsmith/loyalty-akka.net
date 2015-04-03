using System;
using System.Collections.Generic;

namespace LoyaltyService.FraudDetection.Messages
{
	public class SiftScoreResponse : SiftBaseResponse
	{
		public string user_id { get; set; }
		public float score { get; set; }
		public List<SiftScoreReason> reasons { get; set; }
		public SiftLatestLabel latest_label { get; set; }

		public SiftScoreResponse()
		{
			reasons = new List<SiftScoreReason> ();
		}

		public class SiftScoreReason
		{
			public string name { get; set; }
			public string value { get; set; }
			public SiftScoreReasonDetails details { get; set; }

			public class SiftScoreReasonDetails
			{
				public string users { get; set; }
			}
		}

		public class SiftLatestLabel
		{
			public bool is_bad { get; set; }
			public DateTime time { get; set; }
			public List<string> reasons { get; set; }
			public string description { get; set; }

			public SiftLatestLabel()
			{
				reasons = new List<string>();
			}
		}
	}
}
