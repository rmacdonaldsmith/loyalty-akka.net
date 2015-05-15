using System;
using Akka.Actor;
using RestSharp;

namespace LoyaltyService
{
    public class GiftService : ReceiveActor
    {
        private readonly IRestClient _restClient;
        private readonly IActorRef _processBroker;

        public GiftService(IRestClient restClient, IActorRef processBroker)
        {
            _restClient = restClient;
            _processBroker = processBroker;

            Receive<Commands.OrderOtGiftCard>(order =>
                {
                    try
                    {
                        var response = _restClient.Post(new RestRequest());
                        var confirmationId = "";
                        _processBroker.Tell(new Events.OtGiftCardOrdered(order.Gpid, order.RedemptionId, confirmationId));
                        //maybe we get a response that indicates that the order failed
                        //in which case tell the process broker that the order failed.
                    }
                    //catch specific exceptions here
                    catch (Exception ex)
                    {
                        _processBroker.Tell(new Events.OtGiftCardOrderFailed(order.Gpid, order.RedemptionId, ex.Message));
                        throw;
                    }
                });
        }
    }
}
