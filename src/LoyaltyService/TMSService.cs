using Akka.Actor;
using RestSharp;

namespace LoyaltyService
{
    public class TMSService : ReceiveActor
    {
        public class NotifyUser
        {
            public string UserEmail { get; set; }
            public string Message { get; set; }
            public long Gpid { get; set; }
        }

        private readonly IRestClient _restClient;

        public TMSService(IRestClient restClient)
        {
            _restClient = restClient;

            Receive<NotifyUser>(notify =>
                {
                    //call TMS to send an email
                });
        }
    }
}
