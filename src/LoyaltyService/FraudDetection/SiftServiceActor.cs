using System;
using Akka.Actor;
using LoyaltyService.FraudDetection.Messages;
using LoyaltyService.User;

namespace LoyaltyService.FraudDetection
{
    public class SiftServiceActor : ReceiveActor
    {
        private readonly SiftService _siftProxy;
        private long _gpid;
        private Guid _redemptionId;

        public SiftServiceActor(SiftService siftProxy)
        {
            _siftProxy = siftProxy;

            Receive<Commands.CheckRequestForFraud>(msg =>
                {
                    _gpid = msg.GPID;
                    _redemptionId = msg.RedemptionId;
                    _siftProxy.SendOrderInformation(msg.UserInfo,
                                                    msg.ReservationsSummary, msg);
                });

            //set a timeout for a few seconds and then go check if Sift has processed the request
            //in real life this needs to be configurable and probably longer than 1 second!
            SetReceiveTimeout(TimeSpan.FromSeconds(1));

            Receive<ReceiveTimeout>(timeout =>
                {
                    var siftScore = _siftProxy.GetUserScore(_gpid);
                    var selection = Context.ActorSelection("/*/fraud-checker");
                    selection.Tell(new Events.SiftScore(_gpid, _redemptionId, siftScore));
                });

            
        }
    }
}
