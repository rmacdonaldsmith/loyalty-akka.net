using System;
using Akka.Actor;
using LoyaltyService.FraudDetection.Messages;
using LoyaltyService.User;

namespace LoyaltyService
{
    public abstract class RedemptionBase
    {
        public long Gpid { get; private set; }
        public Guid RedemptionId { get; private set; }

        protected RedemptionBase(long gpid, Guid redemptionId)
        {
            Gpid = gpid;
            RedemptionId = redemptionId;
        }
    }

    public static class Events
    {
        public class OTGiftCardRedemptionStarted : RedemptionBase
        {
            public OTGiftCardRedemptionStarted(long gpid, Guid redemptionId, string userEmail, int pointsAmount, string ccy)
                : base(gpid, redemptionId)
            {
                UserEmail = userEmail;
                PointsAmount = pointsAmount;
                Ccy = ccy;
            }

            public string Ccy { get; set; }

            public int PointsAmount { get; set; }

            public string UserEmail { get; set; }
        }

        public class RedemptionCompleted : RedemptionBase
        {
            public Guid RedemptionProcessId { get; private set; }

            public RedemptionCompleted(long gpid, Guid redemptionId)
                : base(gpid, redemptionId)
            {
                RedemptionProcessId = redemptionId;
            }
        }

        public class OtGiftCardOrdered : RedemptionBase
        {
            public string ConfirmationNumber { get; set; }

            public OtGiftCardOrdered(long gpid, Guid redemptionId, string confirmationNumber)
                : base(gpid, redemptionId)
            {
                ConfirmationNumber = confirmationNumber;
            }
        }

        public class OtGiftCardOrderFailed : RedemptionBase
        {
            public string Reason { get; set; }

            public OtGiftCardOrderFailed(long gpid, Guid redemptionId, string reason)
                : base(gpid, redemptionId)
            {
                Reason = reason;
            }
        }

        public class UserNotified : RedemptionBase
        {
            public UserNotified(long gpid, Guid redemptionId)
                : base(gpid, redemptionId)
            {
            }
        }

        public class UserInfoResponseEvent : RedemptionBase
        {
            public UserInfo UserInfo { get; set; }
            public ReservationsSummary ReservationsSummary { get; set; }

            public UserInfoResponseEvent(long gpid, Guid redemptionId, UserInfo userInfo, ReservationsSummary reservationsSummary)
                : base(gpid, redemptionId)
            {
                UserInfo = userInfo;
                ReservationsSummary = reservationsSummary;
            }
        }

        public class SiftScore : RedemptionBase
        {
            public int Score { get; private set; }

            public SiftScore(long gpid, Guid redemptionId, int score)
                : base(gpid, redemptionId)
            {
                Score = score;
            }
        }

        public class InsufficientPoints : RedemptionBase
        {
            public InsufficientPoints(long gpid, Guid redemptionId, int pointsRequired, int pointsBalance) 
                : base(gpid, redemptionId)
            {
                PointsRequired = pointsRequired;
                PointsBalance = pointsBalance;
            }

            public int PointsRequired { get; set; }
            public int PointsBalance { get; set; }
        }

        public class GiftCardOrdered : RedemptionBase
        {
            public string ConfirmationNumber { get; set; }

            public GiftCardOrdered(Guid redemptionId, long gpid, string confirmationNumber)
                : base(gpid, redemptionId)
            {
                ConfirmationNumber = confirmationNumber;
            }
        }

        public class PointsBalanceResult : RedemptionBase
        {
            public int PointsBalance { get; private set; }

            public PointsBalanceResult(long gpid, Guid redemptionId, int balance)
                : base(gpid, redemptionId)
            {
                PointsBalance = balance;
            }
        }

        public class FraudCheckPassed : RedemptionBase
        {
            public FraudCheckPassed(long gpid, Guid redemptionId)
                : base(gpid, redemptionId)
            {

            }
        }

        public class FraudCheckPendingManualReview : RedemptionBase
        {
            public string FraudCheckReason { get; private set; }

            public FraudCheckPendingManualReview(long gpid, Guid redemptionId, string fraudCheckReason)
                : base(gpid, redemptionId)
            {
                FraudCheckReason = fraudCheckReason;
            }
        }

        public class FraudCheckFailed : RedemptionBase
        {
            public string FraudCheckReason { get; private set; }

            public FraudCheckFailed(long gpid, Guid redemptionId, string fraudCheckReason)
                : base(gpid, redemptionId)
            {
                FraudCheckReason = fraudCheckReason;
            }
        }
    }

    public static class Commands
    {
        public class StartOTGiftCardRedemption : RedemptionBase
        {
            public StartOTGiftCardRedemption(long gpid, Guid redemptionId, string ccy, int pointsAmount, string userEmail)
                : base(gpid, redemptionId)
            {
                UserEmail = userEmail;
                PointsAmount = pointsAmount;
                CCY = ccy;
            }

            public string CCY { get; private set; }
            public int PointsAmount { get; private set; }
            public string UserEmail { get; private set; }
        }

        public class OrderOtGiftCard : RedemptionBase
        {
            public OrderOtGiftCard(long gpid, Guid redemptionId, string userEmail, int pointsAmount, string ccy)
                : base(gpid, redemptionId)
            {
                UserEmail = userEmail;
                PointsAmount = pointsAmount;
                CCY = ccy;
            }

            public string CCY { get; private set; }
            public int PointsAmount { get; private set; }
            public string UserEmail { get; private set; }
        }

        public class NotifyUser : RedemptionBase
        {
            public NotifyUser(long gpid, Guid redemptionId, string userEmail, string message)
                : base(gpid, redemptionId)
            {
                UserEmail = userEmail;
                Message = message;
            }

            public string UserEmail { get; set; }
            public string Message { get; set; }
        }

        public class GetUserInfo : RedemptionBase
        {
            public IActorRef FraudCheckerActor { get; set; }

            public GetUserInfo(long gpid, Guid redemptionId, IActorRef fraudCheckerActor)
                : base(gpid, redemptionId)
            {
                FraudCheckerActor = fraudCheckerActor;
            }
        }

        public class SubmitFraudCheck : RedemptionBase
        {
            public SubmitFraudCheck(long gpid, Guid redemptionId)
                : base(gpid, redemptionId)
            {
            }
        }

        public class CheckRequestForFraud : RedemptionBase
        {
            public long GPID { get; set; }
            public string SessionId { get; set; }
            public string Email { get; set; }
            public int PointsToRedeem { get; set; }
            public UserInfo UserInfo { get; set; }
            public Gift Gift { get; set; }
            public ReservationsSummary ReservationsSummary { get; set; }

            public CheckRequestForFraud(Guid redemptionId, long gpid, string email, int pointsToRedeem,
                                        UserInfo userInfo, Gift gift, ReservationsSummary reservationsSummary)
                : base(gpid, redemptionId)
            {
                GPID = gpid;
                Email = email;
                PointsToRedeem = pointsToRedeem;
                UserInfo = userInfo;
                Gift = gift;
                ReservationsSummary = reservationsSummary;
            }
        }

        public class CheckPointsBalance : RedemptionBase
        {
            public CheckPointsBalance(long gpid, Guid redemptionId)
                : base(gpid, redemptionId)
            {
            }
        }

        public class DoFraudCheck : RedemptionBase
        {
            public string EmailAddress { get; set; }
            public int PointsToRedeem { get; set; }
            public Gift Gift { get; set; }

            public DoFraudCheck(long gpid, Guid redemptionId, string emailAddress, int pointsTpRedeem, Gift gift)
                : base(gpid, redemptionId)
            {
                EmailAddress = emailAddress;
                PointsToRedeem = pointsTpRedeem;
                Gift = gift;
            }
        }
    }
}
