using System;
using Akka.Actor;
using RestSharp;

namespace LoyaltyService
{
    public class UserService : ReceiveActor
    {
        #region messages

        public class GetUserInfo : Messages.RedemptionBase
        {
            public GetUserInfo(long gpid) 
                : base(gpid)
            {
                
            }
        }

        public class UserInfoResponse : Messages.RedemptionBase
        {
            public string EmailAddress { get; set; }
            public Guid RedemptionId { get; set; }

            public UserInfoResponse(long gpid) 
                : base(gpid)
            {
            }
        }

        #endregion


        private readonly IRestClient _restClient;
        private readonly ActorRef _processBroker;

        public UserService(IRestClient restClient, ActorRef processBroker)
        {
            _restClient = restClient;
            _processBroker = processBroker;

            //we probably want another actor to aggregate the user data that we send to Sift
        }
    }
}
