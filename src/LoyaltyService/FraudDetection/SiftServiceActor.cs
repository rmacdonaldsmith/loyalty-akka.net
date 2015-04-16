using System;
using Akka.Actor;
using LoyaltyService.FraudDetection.Messages;
using RestSharp;

namespace LoyaltyService.FraudDetection
{
    public class SiftServiceActor : ReceiveActor
    {
        private readonly SiftService _siftProxy;
        private readonly ActorRef _fraudCheckActor;

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

            public CheckRequestForFraud(Guid redemptionId, long gpid, string email, int pointsToRedeem, UserInfo userInfo, Gift gift) 
                : base(gpid)
            {
                RedemptionId = redemptionId;
                GPID = gpid;
                Email = email;
                PointsToRedeem = pointsToRedeem;
                UserInfo = userInfo;
                Gift = gift;
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

        public SiftServiceActor(ActorRef fraudCheckActor, SiftService siftProxy)
        {
            _fraudCheckActor = fraudCheckActor;
            _siftProxy = siftProxy;

            Receive<SubmitFraudCheck>(msg =>
                {
                    _siftProxy.SendOrderInformation();
                });
        }
    }
}
