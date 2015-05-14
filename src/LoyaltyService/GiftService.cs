using System;
using Akka.Actor;
using RestSharp;

namespace LoyaltyService
{
    public class GiftService : ReceiveActor
    {
        private readonly IRestClient _restClient;
        private readonly IActorRef _processBroker;

        public class OrderOtGiftCard : Messages.RedemptionBase
        {
            public OrderOtGiftCard(long gpid, string userEmail, int pointsAmount, string ccy)
                : base(gpid)
            {
                UserEmail = userEmail;
                PointsAmount = pointsAmount;
                CCY = ccy;
            }

            public string CCY { get; private set; }
            public int PointsAmount { get; private set; }
            public string UserEmail { get; private set; }
        }

        public class OtGiftCardOrdered : Messages.RedemptionBase
        {
            public string ConfirmationNumber { get; set; }

            public OtGiftCardOrdered(long gpid, string confirmationNumber)
                : base(gpid)
            {
                ConfirmationNumber = confirmationNumber;
            }
        }

        public class OtGiftCardOrderFailed : Messages.RedemptionBase
        {
            public string Reason { get; set; }

            public OtGiftCardOrderFailed(long gpid, string reason)
                : base(gpid)
            {
                Reason = reason;
            }
        }

        public GiftService(IRestClient restClient, IActorRef processBroker)
        {
            _restClient = restClient;
            _processBroker = processBroker;

            Receive<OrderOtGiftCard>(order =>
                {
                    try
                    {
                        var response = _restClient.Post(new RestRequest());
                        var confirmationId = "";
                        _processBroker.Tell(new OtGiftCardOrdered(order.Gpid, confirmationId));
                        //maybe we get a response that indicates that the order failed
                        //in which case tell the process broker that the order failed.
                    }
                    //catch specific exceptions here
                    catch (Exception ex)
                    {
                        _processBroker.Tell(new OtGiftCardOrderFailed(order.Gpid, ex.Message));
                        throw;
                    }
                });
        }
    }
}
