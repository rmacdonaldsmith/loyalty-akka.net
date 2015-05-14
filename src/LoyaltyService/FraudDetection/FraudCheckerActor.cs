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

        private readonly IActorRef _userService;
        private readonly IActorRef _processBroker;
        private readonly IActorRef _siftService;
        private DoFraudCheck _doFraudCheck;

        /// <summary>
        /// The fraud checker actor is its own mini-process manager, coordinating the process
        /// of gettting user information and submitting it to the sift service. Hence, this actor
        /// deals directly with the User service and Sift service, finally relaying the result
        /// to the Process Broker (who forwards on to the Process State actor).
        /// </summary>
        /// <param name="processBroker"></param>
        /// <param name="userServiceActor"></param>
        /// <param name="siftServiceActor"></param>
        public FraudCheckerActor(IActorRef processBroker, IActorRef userServiceActor, IActorRef siftServiceActor)
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
