using System;
using Akka.Actor;
using LoyaltyService.User;

namespace LoyaltyService.FraudDetection
{
    public class FraudCheckerActor : ReceiveActor
    {
        private readonly IActorRef _userService;
        private readonly IActorRef _processBroker;
        private readonly IActorRef _siftService;
        private Commands.DoFraudCheck _doFraudCheck;

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
            Receive<Commands.DoFraudCheck>(msg =>
                {
                    _doFraudCheck = msg;
                    _userService.Tell(new Commands.GetUserInfo(msg.Gpid, msg.RedemptionId, Self));
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
            Receive<Events.UserInfoResponseEvent>(userInfo =>
                                                       _siftService.Tell(
                                                           new Commands.CheckRequestForFraud(
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
            Receive<Events.SiftScore>(score => _processBroker.Tell(score));
        }
    }
}
