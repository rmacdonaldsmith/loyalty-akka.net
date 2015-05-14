using System;
using Akka.Actor;
using LoyaltyService.FraudDetection.Messages;

namespace LoyaltyService.User
{
    public class UserServiceActor : ReceiveActor
    {
        #region messages

        public class GetUserInfo : Messages.RedemptionBase
        {
            public Guid RedemptionId { get; set; }
            public IActorRef FraudCheckerActor { get; set; }

            public GetUserInfo(long gpid, Guid redemptionId, IActorRef fraudCheckerActor) 
                : base(gpid)
            {
                RedemptionId = redemptionId;
                FraudCheckerActor = fraudCheckerActor;
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
        private readonly IActorRef _fraudCheckerActor;

        public UserServiceActor(UserService userService)
        {
            _userService = userService;

            Receive<GetUserInfo>(getUserInfo =>
                {
                    var userInfo = _userService.GetUserInfo(getUserInfo.Gpid);
                    var resos = _userService.GetReservationsSummary(getUserInfo.Gpid);

                    getUserInfo.FraudCheckerActor.Tell(new UserInfoResponse(getUserInfo.Gpid, getUserInfo.RedemptionId, userInfo, resos));
                });
        }

    }
}
