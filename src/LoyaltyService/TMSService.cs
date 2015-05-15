using Akka.Actor;
using RestSharp;

namespace LoyaltyService
{
    public class TmsService : ReceiveActor
    {
        private readonly IRestClient _restClient;

        public TmsService(IRestClient restClient)
        {
            _restClient = restClient;

            Receive<Commands.NotifyUser>(notify =>
                {
                    //if contacting TMS was successful
                    if(true)
                        Sender.Tell(new Events.UserNotified(notify.Gpid, notify.RedemptionId));
//                    else
//                        Sender.Tell(somethingelse);
                });
        }
    }
}
