using System;
using System.Collections.Generic;
using LoyaltyService.FraudDetection.Messages;

namespace LoyaltyService.User
{
	public class UserInfoResponse
	{
		public long GlobalPersonId { get; set; }

		public int UserID { get; set; }

		public bool IsActive { get; set; }

		public OTUserStatus UserStatus { get; set; }

		public OTUserType UserType { get; set; }

		public int CompanyId { get; set; }

		public string LoginName { get; set; }

		public string Email { get; set; }

		public int Points { get; set; }

		public bool PointsAllowed { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string SortableFirstName { get; set; }

		public string SortableLastName { get; set; }

		public string Address1 { get; set; }

		public string Address2 { get; set; }

		public string State { get; set; }

		public string Zip { get; set; }

		public string City { get; set; }

		public string CountryID { get; set; }

		public int MetroId { get; set; }

		public string DefaultRequests { get; set; }

		public bool DiningFormOptIn { get; set; }

		public DateTime CreateDate { get; set; }

		public List<PhoneNumber> PhoneNumbers { get; set; }

		public List<SocialInfo> SocialInfos { get; set; }

		public UserInfoResponse()
		{
			PhoneNumbers = new List<PhoneNumber>();
			SocialInfos = new List<SocialInfo>();
		}

		public class PhoneNumber
		{
			public enum OTPhoneNumberType
			{
				Work,
				Mobile
			}

			public OTPhoneNumberType PhoneNumberType { get; set; }

			public string CountryID { get; set; }

			public string Number { get; set; }

			public string Extension { get; set; }
		}

		public class SocialInfo
		{
			public string SocialUid { get; set; }

			public string SocialAccessToken { get; set; }

			public int SocialType { get; set; }
		}
	}
}
