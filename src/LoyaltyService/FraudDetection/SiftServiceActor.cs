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

        public class FraudCheckPassed : LoyaltyService.Messages.RedemptionBase
        {
            public FraudCheckPassed(long gpid)
                : base(gpid)
            {

            }
        }

        public class FraudCheckPendingManualReview : LoyaltyService.Messages.RedemptionBase
        {
            public string FraudCheckReason { get; private set; }

            public FraudCheckPendingManualReview(long gpid, string fraudCheckReason)
                : base(gpid)
            {
                FraudCheckReason = fraudCheckReason;
            }
        }

        public class FraudCheckFailed : LoyaltyService.Messages.RedemptionBase
        {
            public string FraudCheckReason { get; private set; }

            public FraudCheckFailed(long gpid, string fraudCheckReason)
                : base(gpid)
            {
                FraudCheckReason = fraudCheckReason;
            }
        }

        # endregion

        private readonly SiftService _siftProxy;
        private readonly ActorRef _fraudCheckActor;
        private long _gpid;

        public SiftServiceActor(ActorRef fraudCheckActor, SiftService siftProxy)
        {
            _fraudCheckActor = fraudCheckActor;
            _siftProxy = siftProxy;


            Receive<CheckRequestForFraud>(msg =>
                {
                    _gpid = msg.GPID;
                    _siftProxy.SendOrderInformation(msg.UserInfo,
                                                    msg.ReservationsSummary, msg);
                });

            //set a timeout for a few seconds and then go check if Sift has processed the request
            SetReceiveTimeout(TimeSpan.FromSeconds(3));

            Receive<ReceiveTimeout>(timeout =>
                {
                    var siftScore = _siftProxy.GetUserScore(_gpid);
                    _fraudCheckActor.Tell(siftScore);
                });

            
        }

    }
}
