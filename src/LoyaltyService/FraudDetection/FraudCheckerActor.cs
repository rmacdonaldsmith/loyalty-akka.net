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
            public int PointsTpRedeem { get; set; }
            public Gift Gift { get; set; }

            public DoFraudCheck(long gpid, Guid redemptionId, string emailAddress, int pointsTpRedeem, Gift gift) 
                : base(gpid)
            {
                RedemptionId = redemptionId;
                EmailAddress = emailAddress;
                PointsTpRedeem = pointsTpRedeem;
                Gift = gift;
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

            Receive<DoFraudCheck>(msg =>
                {
                    _doFraudCheck = msg;
                    _userService.Tell(new UserServiceActor.GetUserInfo(msg.Gpid, msg.RedemptionId, Self));
                    Become(HandleUserInfoResponse);
                });
        }

        public FraudCheckerActor()
        {
            //for testkit
        }

        private void HandleUserInfoResponse()
        {
            //aggregate user data in here
            Receive<UserServiceActor.UserInfoResponse>(userInfo => _siftService.Tell(
                new SiftServiceActor.CheckRequestForFraud(
                        userInfo.RedemptionId, 
                        userInfo.Gpid, 
                        _doFraudCheck.EmailAddress, 
                        _doFraudCheck.PointsTpRedeem,
                        userInfo.UserInfo,
                        _doFraudCheck.Gift,
                        userInfo.ReservationsSummary
                    )
                )
            );
            Become(HandleSiftResponse);
        }

        private void HandleSiftResponse()
        {
            Receive(new Predicate<object>(msg =>
                                  msg is SiftServiceActor.FraudCheckFailed ||
                                  msg is SiftServiceActor.FraudCheckPassed ||
                                  msg is SiftServiceActor.FraudCheckPendingManualReview),
                    msg => 
                        _processBroker.Tell(msg));
        }
    }
}
