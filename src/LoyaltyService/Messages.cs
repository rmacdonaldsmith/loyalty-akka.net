using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltyService
{
    public static class Messages
    {
        public abstract class RedemptionBase
        {
            public long Gpid { get; private set; }

            protected RedemptionBase(long gpid)
            {
                Gpid = gpid;
            }
        }

        public static class Commands
        {
            public class StartOTGiftCardRedemption : RedemptionBase
            {
                public StartOTGiftCardRedemption(long gpid, string ccy, int pointsAmount, string userEmail)
                    : base (gpid)
                {
                    UserEmail = userEmail;
                    PointsAmount = pointsAmount;
                    CCY = ccy;
                }

                public string CCY { get; private set; }
                public int PointsAmount { get; private set; }
                public string UserEmail { get; private set; }
            }

            public class StartAmazonGiftCardRedemption : RedemptionBase
            {
                public StartAmazonGiftCardRedemption(long gpid)
                    : base(gpid)
                {
                }
            }

            public class DoFraudCheck : RedemptionBase
            {
                public DoFraudCheck(long gpid)
                    : base(gpid)
                {
                }
            }

            public class CheckPointsBalance : RedemptionBase
            {
                public CheckPointsBalance(long gpid)
                    : base(gpid)
                {
                }
            }

        }

        public static class Events
        {
            public class OTGiftCardRedemptionStarted : RedemptionBase
            {
                public Guid RedmeptionProcessId { get; private set; }

                public OTGiftCardRedemptionStarted(long gpid)
                    : base (gpid)
                {
                    
                }
            }

            public class FraudCheckPassed : RedemptionBase
            {
                public FraudCheckPassed(long gpid)
                    : base (gpid)
                {
                    
                }
            }

            public class FraudCheckPendingManualReview : RedemptionBase
            {
                public string FraudCheckReason { get; private set; }

                public FraudCheckPendingManualReview(long gpid, string fraudCheckReason)
                    : base (gpid)
                {
                    FraudCheckReason = fraudCheckReason;
                }
            }

            public class FraudCheckFailed : RedemptionBase
            {
                public string FraudCheckReason { get; private set; }

                public FraudCheckFailed(long gpid, string fraudCheckReason)
                    : base (gpid)
                {
                    FraudCheckReason = fraudCheckReason;
                }
            }
        }
    }
}
