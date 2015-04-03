using System;
using Akka.Actor;
using LoyaltyService.FraudDetection;

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


        public class FraudCheckPassed : Messages.RedemptionBase
        {
            public FraudCheckPassed(long gpid) : base(gpid)
            {
            }
        }

        public class FraudCheckFailed : Messages.RedemptionBase
        {
            public FraudCheckFailed(long gpid) : base(gpid)
            {
            }
        }

        public class FraudCheckPendingManualReview : Messages.RedemptionBase
        {
            public FraudCheckPendingManualReview(long gpid) : base(gpid)
            {
            }
        }

        # endregion

        private readonly ActorRef _userService;
        private ActorRef _processBroker;

        public FraudCheckerActor(ActorRef userService, ActorRef processBroker)
        {
            _userService = userService;
            _processBroker = processBroker;

            Receive<SiftServiceActor.DoFraudCheck>(msg =>
                {
                    _userService.Tell(new UserService.GetUserInfo(msg.Gpid));
                    Become(HandleUserInfoResponse);
                });
        }

        private void HandleUserInfoResponse()
        {
            //aggregate user data in here
            Receive<UserService.UserInfoResponse>(response => _processBroker.Tell(new AggregatedUserData(response.Gpid)
                {
                    EmailAddress = response.EmailAddress, 
                    RedemptionId = response.RedemptionId
                }));
        }
    }
}
