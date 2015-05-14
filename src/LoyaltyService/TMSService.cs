using Akka.Actor;
using RestSharp;

namespace LoyaltyService
{
    public class TmsService : ReceiveActor
    {
        public class NotifyUser : Messages.RedemptionBase
        {
            public NotifyUser(long gpid, string userEmail, string message) 
                : base(gpid)
            {
                UserEmail = userEmail;
                Message = message;
            }

            public string UserEmail { get; set; }
            public string Message { get; set; }
        }

        private readonly IRestClient _restClient;

        public TmsService(IRestClient restClient)
        {
            _restClient = restClient;

            Receive<NotifyUser>(notify =>
                {
                    //if contacting TMS was successful
                    if(true)
                        Sender.Tell(new UserNotified(notify.Gpid));
//                    else
//                        Sender.Tell(somethingelse);
                });
        }

        # region Messages

        public class UserNotified : Messages.RedemptionBase
        {
            public UserNotified(long gpid) 
                : base(gpid)
            {
            }
        }

        # endregion
    }
}
