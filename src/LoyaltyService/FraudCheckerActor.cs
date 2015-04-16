using System;
using Akka.Actor;
using LoyaltyService.FraudDetection;
using LoyaltyService.FraudDetection.Messages;
using LoyaltyService.User;

namespace LoyaltyService
{
    public class FraudCheckerActor : ReceiveActor
    {
        # region Messages

        public class AggregatedUserData : Messages.RedemptionBase
        {
            public Guid RedemptionId { get; set; }
            public string EmailAddress { get; set; }
            public MailingAddress Address { get; set; }

            public AggregatedUserData(long gpid) 
                : base(gpid)
            {
            }
        }

        public class DoFraudCheck : Messages.RedemptionBase
        {
            public Guid RedemptionId { get; set; }

            public DoFraudCheck(long gpid, Guid redemptionId) 
                : base(gpid)
            {
                RedemptionId = redemptionId;
            }
        }

        public class MailingAddress
        {
            public string Street1 { get; set; }
            public string Street2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }

            public static MailingAddress NoAddress()
            {
                return new MailingAddress();
            }
        }

        # endregion

        private readonly ActorRef _userService;
        private readonly ActorRef _processBroker;
        private readonly ActorRef _siftService;

        public FraudCheckerActor(ActorRef userService, ActorRef processBroker, ActorRef siftService)
        {
            _userService = userService;
            _processBroker = processBroker;
            _siftService = siftService;

            Receive<DoFraudCheck>(msg =>
                {
                    _userService.Tell(new UserServiceActor.GetUserInfo(msg.Gpid, msg.RedemptionId));
                    Become(HandleUserInfoResponse);
                });
        }

        private void HandleUserInfoResponse()
        {
            //aggregate user data in here
            Receive<UserServiceActor.UserInfoResponse>(userInfo =>
                {
                    _siftService.Tell(new SiftServiceActor.CheckRequestForFraud(
                        userInfo.RedemptionId, userInfo.Gpid,
                        ));
                });
        }

        private void HandleSiftResponse()
        {
            Receive<UserServiceActor.UserInfoResponse>(response => _processBroker.Tell(new AggregatedUserData(response.Gpid)
            {
                EmailAddress = response.EmailAddress,
                RedemptionId = response.RedemptionId
            }));
        }
    }
}
