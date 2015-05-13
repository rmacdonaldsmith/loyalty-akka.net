using System;
using Akka.Actor;
using LoyaltyService.FraudDetection.Messages;
using LoyaltyService.User;

namespace LoyaltyService.FraudDetection
{
    public class SiftServiceActor : ReceiveActor
    {
        # region Messages

        public class SubmitFraudCheck : LoyaltyService.Messages.RedemptionBase
        {
            public SubmitFraudCheck(long gpid)
                : base(gpid)
            {
            }
        }

        public class CheckRequestForFraud : LoyaltyService.Messages.RedemptionBase
        {
            public Guid RedemptionId { get; set; }
            public long GPID { get; set; }
            public string SessionId { get; set; }
            public string Email { get; set; }
            public int PointsToRedeem { get; set; }
            public UserInfo UserInfo { get; set; }
            public Gift Gift { get; set; }
            public ReservationsSummary ReservationsSummary { get; set; }

            public CheckRequestForFraud(Guid redemptionId, long gpid, string email, int pointsToRedeem, 
                UserInfo userInfo, Gift gift, ReservationsSummary reservationsSummary) 
                : base(gpid)
            {
                RedemptionId = redemptionId;
                GPID = gpid;
                Email = email;
                PointsToRedeem = pointsToRedeem;
                UserInfo = userInfo;
                Gift = gift;
                ReservationsSummary = reservationsSummary;
            }
        }

        public class SiftScore : LoyaltyService.Messages.RedemptionBase
        {
            public int Score { get; private set; }

            public SiftScore(long gpid, int score) 
                : base(gpid)
            {
                Score = score;
            }
        }

        # endregion

        private readonly SiftService _siftProxy;
        private long _gpid;

        public SiftServiceActor(SiftService siftProxy)
        {
            _siftProxy = siftProxy;

            Receive<CheckRequestForFraud>(msg =>
                {
                    _gpid = msg.GPID;
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
                    selection.Tell(new SiftServiceActor.SiftScore(_gpid, siftScore));
                });

            
        }
    }
}
