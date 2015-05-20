using System;
using Akka.Actor;
using Akka.Persistence;
using LoyaltyService.Common;

namespace LoyaltyService.FraudDetection
{
    public class RedemptionPersistenceActor : PersistentActor
    {
        private readonly IActorRef _processBroker;
        private readonly TypeMatch _eventHandlers;
        private Guid _redemptionId;
        private long _gpid;
        private int _pointsRequired;
        private string _userEmail;
        private string _ccy;
        private States _redemptionState;

        public override string PersistenceId
        {
            get { return _redemptionId.ToString(); }
        }

        public RedemptionPersistenceActor()
        {
            _eventHandlers = new TypeMatch()
                .Case((Events.OTGiftCardRedemptionStarted started) => Apply(started))
                .Default(TypeMatch.Throw); //probably want to throw - if we miss a case to handle an event then than implies that we have not coded our actor properly. it should handle all events that it raises
        }

        protected override bool ReceiveRecover(object message)
        {
            //events are replayed in here when starting, restarting after a crash or migrating to a
            //new cluster
            _eventHandlers.Match(message);
            return true;
        }

        //all messages from the inbound mailbox get dequeued in to here - do your command processing here
        protected override bool ReceiveCommand(object message)
        {
            var start = message as Commands.StartOTGiftCardRedemption;
            if (start == null)
                return false;

            return HandleStartRedemptionCommand(start);
        }

        private bool HandleStartRedemptionCommand(Commands.StartOTGiftCardRedemption start)
        {
            if (_redemptionState != States.StartingRedemption)
                return false;

            Persist(
                new Events.OTGiftCardRedemptionStarted(start.Gpid, start.RedemptionId, start.UserEmail,
                                                       start.PointsAmount, start.CCY), Apply);
            Become(HandleSiftScore);
            return true;
        }

        private bool HandleSiftScore(object message)
        {
            var score = message as Events.SiftScore;
            if (score == null)
                return false;

            if (score.Score < 50)
            {
                Persist(new Events.FraudCheckFailed(score.Gpid, score.RedemptionId, "Score too low"), Apply);
                return true;
                
            }
            
            if (score.Score < 70)
            {
                Persist(new Events.FraudCheckPendingManualReview(score.Gpid, score.RedemptionId,
                                                                  "Score not quite high enough"), Apply);
                return true;
            }

            Persist(new Events.FraudCheckPassed(score.Gpid, score.RedemptionId), Apply);
            Become(HandlePointsBalanceResult);

            return false;
        }

        private bool HandlePointsBalanceResult(object message)
        {
            var balance = message as Events.PointsBalanceResult;
            if (balance == null)
                return false;

            if (balance.PointsBalance >= _pointsRequired)
            {
                _processBroker.Tell(new Commands.OrderOtGiftCard(_gpid, _redemptionId, _userEmail, _pointsRequired, _ccy));
                Become(HandleGiftOrderConfirmation);
            }
            else
            {
                Persist(new Events.InsufficientPoints(_gpid, _redemptionId,
                                                      balance.PointsBalance, _pointsRequired), Apply);
            }

            return true;
        }

        private bool HandleGiftOrderConfirmation(object message)
        {
            var orderConfirmation = message as Events.GiftCardOrdered;
            if (orderConfirmation == null)
                return false;

            Persist(new Events.OtGiftCardOrdered(_gpid, _redemptionId, orderConfirmation.ConfirmationNumber), Apply);
            Become(HandleUserNotified);

            return true;
        }

        private bool HandleUserNotified(object message)
        {
            var notified = message as Events.UserNotified;
            if (notified == null)
                return false;

            Persist(new Events.RedemptionCompleted(_gpid, _redemptionId), Apply);

            return true;
        }

        private void Apply(Events.RedemptionCompleted completed)
        {
            _redemptionState = States.CompletedOrdered;
            _processBroker.Tell(completed);
        }


        private void Apply(Events.OtGiftCardOrdered ordered)
        {
            _redemptionState = States.GiftCardOrdered;
            _processBroker.Tell(new Commands.NotifyUser(ordered.Gpid, ordered.RedemptionId, _userEmail,
                                                        "Confirmation number: " + ordered.ConfirmationNumber));

        }

        private void Apply(Events.InsufficientPoints insufficientPoints)
        {
            _redemptionState = States.OrderingGiftCard;
            _processBroker.Tell(new Events.InsufficientPoints(_gpid, _redemptionId,
                                                              insufficientPoints.PointsBalance, _pointsRequired));
        }

        private void Apply(Events.FraudCheckPendingManualReview review)
        {
            _redemptionState = States.FraudCheckPendingManualReview;
            _processBroker.Tell(new Events.OtGiftCardOrderFailed(_gpid, _redemptionId, "Manual review of user required"));
        }

        private void Apply(Events.FraudCheckPassed passed)
        {
            _redemptionState = States.FraudCheckPassed;
            _processBroker.Tell(new Commands.CheckPointsBalance(passed.Gpid, passed.RedemptionId));
            Become(HandlePointsBalanceResult);
        }

        private void Apply(Events.FraudCheckFailed failed)
        {
            _redemptionState = States.FraudCheckFailed;
            _processBroker.Tell(new Events.OtGiftCardOrderFailed(_gpid, _redemptionId, "Fraud check score was too low"));
        }

        private void Apply(Events.OTGiftCardRedemptionStarted started)
        {
            _gpid = started.Gpid;
            _redemptionId = Guid.NewGuid();
            _redemptionState = States.StartingRedemption;
            _pointsRequired = started.PointsAmount;
            _userEmail = started.UserEmail;
            _ccy = started.Ccy;
        }
    }
}
