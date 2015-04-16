using System;
using Akka.Actor;
using LoyaltyService.FraudDetection.Messages;
using RestSharp;

namespace LoyaltyService.User
{
    public class UserServiceActor : ReceiveActor
    {
        #region messages

        public class GetUserInfo : Messages.RedemptionBase
        {
            public Guid RedemptionId { get; set; }

            public GetUserInfo(long gpid, Guid redemptionId) 
                : base(gpid)
            {
                RedemptionId = redemptionId;
            }
        }

        public class UserInfoResponse : Messages.RedemptionBase
        {
            public Guid RedemptionId { get; set; }
            public UserInfo UserInfo { get; set; }
            public ReservationsSummary ReservationsSummary { get; set; }

            public UserInfoResponse(long gpid, Guid redemptionId, UserInfo userInfo, ReservationsSummary reservationsSummary) 
                : base(gpid)
            {
                RedemptionId = redemptionId;
                UserInfo = userInfo;
                ReservationsSummary = reservationsSummary;
            }
        }

        #endregion


        private readonly UserService _userService;
        private readonly ActorRef _fraudCheckerActor;

        public UserServiceActor(ActorRef fraudCheckerActor, UserService userService)
        {
            _fraudCheckerActor = fraudCheckerActor;
            _userService = userService;

            Receive<GetUserInfo>(getUserInfo =>
                {
                    var userInfo = _userService.GetUserInfo(getUserInfo.Gpid);
                    var resos = _userService.GetReservationsSummary(getUserInfo.Gpid);

                    _fraudCheckerActor.Tell(new UserInfoResponse(getUserInfo.Gpid, getUserInfo.RedemptionId, userInfo, resos));
                });
        }
    }
}
