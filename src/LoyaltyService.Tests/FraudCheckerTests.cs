﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit;
using LoyaltyService.FraudDetection;
using LoyaltyService.FraudDetection.Messages;
using LoyaltyService.User;
using RestSharp;
using Xunit;

namespace LoyaltyService.Tests
{
    public class FraudCheckerTests : TestKit
    {
        private readonly ActorRef _fraudChecker;
        private readonly IRestClient _userRestClient;
        private readonly IRestClient _siftRestClient;
        private readonly UserService _userService;
        private readonly SiftService _siftService;
        private readonly Guid _redemptionId = Guid.NewGuid();
        private const long Gpid = 1234;
        private const string Ccy = "USD";
        private const int PointsToRedeem = 1000;
        private const string EmailAddress = "user@domain.com";

        public FraudCheckerTests()
        {
            _userRestClient = new FakeRestClient(request => UserInfoResponse());
            _userRestClient = new FakeRestClient(request =>
                {
                    if (request.Resource.Contains("/user/v1/users/"))
                        return this.UserInfoResponse();

                    if (request.Resource.Contains("/user/v2/users/"))
                        return UserReservations();

                    throw new Exception("Unexpected request");
                });
            _siftRestClient = new FakeRestClient(request => SiftResponse());
            _userService = new UserService(_userRestClient);
            _siftService = new SiftService(_siftRestClient, "fake-api-key");
            var userServiceActorProps = Props.Create(() => new UserServiceActor(_userService));
            var siftServiceActorProps = Props.Create(() => new SiftServiceActor(_fraudChecker, _siftService));
            var userServiceActorRef = ActorOf(userServiceActorProps);
            var siftServiceActorRef = ActorOf(siftServiceActorProps);

            var fraudCheckProps =
                Props.Create(() => new FraudCheckerActor(TestActor, userServiceActorRef, siftServiceActorRef));
            _fraudChecker = ActorOf(fraudCheckProps, "fraud-checker");
        }

        [Fact(DisplayName = "When perfomring a fraud check")]
        public void When_performing_the_fraud_check()
        {
            _fraudChecker.Tell(new FraudCheckerActor.DoFraudCheck(Gpid, _redemptionId, EmailAddress, PointsToRedeem, new Gift(PointsToRedeem, Ccy)));

            ExpectMsg<SiftServiceActor.FraudCheckPassed>();
        }

        private readonly Func<RestResponse<UserInfoResponse>> UserInfoResponse = () => new RestResponse<UserInfoResponse>
            {
                Data = new UserInfoResponse
                    {
                        GlobalPersonId = 10000321,
                        LoginName = "TestLogin",
                        Email = "test@opentable.com",
                        FirstName = "first name",
                        LastName = "last name",
                        Points = 100000,
                        UserType = OTUserType.Registered,
                        CreateDate = DateTime.Now,
                        Address1 = "Address1",
                        Address2 = "Address2",
                        City = "City",
                        Zip = "ZIP",
                        CountryID = "US",
                        PhoneNumbers = new System.Collections.Generic.List<UserInfoResponse.PhoneNumber>
                            {
                                new UserInfoResponse.PhoneNumber
                                    {
                                        PhoneNumberType = User.UserInfoResponse.PhoneNumber.OTPhoneNumberType.Mobile,
                                        Number = "0123456789"
                                    }
                            }
                    },
                StatusCode = HttpStatusCode.Created,
                ResponseStatus = ResponseStatus.Completed
            };

        private readonly Func<RestResponse<SiftScoreResponse>> SiftResponse = () => new RestResponse<SiftScoreResponse>
            {
                Data = new SiftScoreResponse
                    {
                        status = 0,
                        error_message = "OK",
                        time = 1424218795,
                        request =
                            "{\\\"$type\\\": \\\"$create_order\\\",\\\"$api_key\\\": \\\"82f6d7e6ddba0e9f\\\",\\\"$user_id\\\": \\\"testuserid\\\",\\\"$session_id\\\": \\\"restClientTest1\\\",\\\"$order_id\\\": \\\"testingorder1\\\",\\\"$user_email\\\": \\\"testemail@opentable.com\\\"}"
                    },
                StatusCode = HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed
            };

        private readonly Func<RestResponse<UserTransactionsResponse>> UserReservations = () => new RestResponse<UserTransactionsResponse>
            {
                Data = new UserTransactionsResponse
                    {
                        GlobalPersonId = 10000321,
                        Transactions = new System.Collections.Generic.List<UserTransactionsResponse.UserTransaction>
                            {
                                new UserTransactionsResponse.UserTransaction
                                    {
                                        Type = UserTransactionsResponse.TransactionType.Reservation,
                                        DateTimeUtc = DateTime.UtcNow,
                                        ReservationState = UserTransactionsResponse.ReservationState.Seated
                                    },
                                new UserTransactionsResponse.UserTransaction
                                    {
                                        Type = UserTransactionsResponse.TransactionType.Points,
                                        DateTimeUtc = DateTime.UtcNow
                                    }
                            }
                    },
                StatusCode = HttpStatusCode.Created,
                ResponseStatus = ResponseStatus.Completed
            };
    }
}
