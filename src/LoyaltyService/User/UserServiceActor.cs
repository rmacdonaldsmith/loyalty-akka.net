using Akka.Actor;

namespace LoyaltyService.User
{
    public class UserServiceActor : ReceiveActor
    {
        private readonly UserService _userService;
        private readonly IActorRef _fraudCheckerActor;

        public UserServiceActor(UserService userService)
        {
            _userService = userService;

            Receive<Commands.GetUserInfo>(getUserInfo =>
                {
                    var userInfo = _userService.GetUserInfo(getUserInfo.Gpid);
                    var resos = _userService.GetReservationsSummary(getUserInfo.Gpid);

                    getUserInfo.FraudCheckerActor.Tell(new Events.UserInfoResponseEvent(getUserInfo.Gpid, getUserInfo.RedemptionId, userInfo, resos));
                });
        }

    }
}
