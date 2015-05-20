using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltyService
{
    public enum States
    {
        StartingRedemption,
        PerformingFraudCheck,
        FraudCheckPassed,
        FraudCheckFailed,
        FraudCheckPendingManualReview,
        CheckingPointsBalance,
        OrderingGiftCard,
        GiftCardOrdered,
        CompletedOrdered,
        None
    }

    public class RedemptionState
    {
        private Guid _redemptionId;
        private long _gpid;
        private int _pointsRequired;
        private string _userEmail;
        private string _ccy;
        private States _currentState = States.None;

        public RedemptionState()
        {
            
        }

        
    }
}
