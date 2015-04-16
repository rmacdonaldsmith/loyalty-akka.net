using System;

namespace LoyaltyService.FraudDetection.Messages
{
	public class UserInfo
	{
		public long GPID { get; set; }
		public string LoginName { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string PhoneNumber { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string City { get; set; }
		public string Zip { get; set; }
		public string CountryID { get; set; }
		public int Points { get; set; }
		public OTUserType UserType { get; set; }
		public DateTime CreateDate { get; set; }
	}
}
