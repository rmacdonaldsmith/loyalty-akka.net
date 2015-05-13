using System;
using Akka.Actor;
using LoyaltyService.User;

namespace LoyaltyService.FraudDetection
{
    public class FraudCheckerActor : ReceiveActor
    {
        # region Messages

        public class DoFraudCheck : LoyaltyService.Messages.RedemptionBase
        {
            public Guid RedemptionId { get; set; }
            public string EmailAddress { get; set; }
            public int PointsToRedeem { get; set; }
            public Gift Gift { get; set; }

            public DoFraudCheck(long gpid, Guid redemptionId, string emailAddress, int pointsTpRedeem, Gift gift) 
                : base(gpid)
            {
                RedemptionId = redemptionId;
                EmailAddress = emailAddress;
                PointsToRedeem = pointsTpRedeem;
                Gift = gift;
            }
        }

        public class FraudCheckPassed : LoyaltyService.Messages.RedemptionBase
        {
            public FraudCheckPassed(long gpid)
                : base(gpid)
            {

            }
        }

        public class FraudCheckPendingManualReview : LoyaltyService.Messages.RedemptionBase
        {
            public string FraudCheckReason { get; private set; }

            public FraudCheckPendingManualReview(long gpid, string fraudCheckReason)
                : base(gpid)
            {
                FraudCheckReason = fraudCheckReason;
            }
        }

        public class FraudCheckFailed : LoyaltyService.Messages.RedemptionBase
        {
            public string FraudCheckReason { get; private set; }

            public FraudCheckFailed(long gpid, string fraudCheckReason)
                : base(gpid)
            {
                FraudCheckReason = fraudCheckReason;
            }
        }

        # endregion

        private readonly ActorRef _userService;
        private readonly ActorRef _processBroker;
        private readonly ActorRef _siftService;
        private DoFraudCheck _doFraudCheck;

        public FraudCheckerActor(ActorRef processBroker, ActorRef userServiceActor, ActorRef siftServiceActor)
        {
            _processBroker = processBroker;
            _userService = userServiceActor;
            _siftService = siftServiceActor;

            //first we need to get some user details from the user service
            Receive<DoFraudCheck>(msg =>
                {
                    _doFraudCheck = msg;
                    _userService.Tell(new UserServiceActor.GetUserInfo(msg.Gpid, msg.RedemptionId, Self));
                    Become(HandleUserServiceResponse);
                });

            ReceiveAny(m =>
                {
                    throw new Exception("Unexpected message was received");
                });
        }

        public FraudCheckerActor()
        {
            //for testkit
        }

        private void HandleUserServiceResponse()
        {
            //then we call out to the sift service to return us a score
            Receive<UserServiceActor.UserInfoResponse>(userInfo =>
                                                       _siftService.Tell(
                                                           new SiftServiceActor.CheckRequestForFraud(
                                                               userInfo.RedemptionId,
                                                               userInfo.Gpid,
                                                               _doFraudCheck.EmailAddress,
                                                               _doFraudCheck.PointsToRedeem,
                                                               userInfo.UserInfo,
                                                               _doFraudCheck.Gift,
                                                               userInfo.ReservationsSummary
                                                               )
                                                           )
                );

            //get the response from the sift service with the fraud score, forward to the broker
            //we will leave it to the process state actor to make decisions based on the score
            Receive<SiftServiceActor.SiftScore>(score => _processBroker.Tell(score));
        }
    }
}
