using Akka.Actor;
using RestSharp;

namespace LoyaltyService.FraudDetection
{
    public class SiftServiceActor : ReceiveActor
    {
        private readonly SiftService _siftProxy;
        private readonly IRestClient _restClient;
        private readonly ActorRef _processBroker;

        public SiftServiceActor(SiftService siftProxy)
        {
            _siftProxy = siftProxy;
        }

        public class DoFraudCheck : LoyaltyService.Messages.RedemptionBase
        {
            public DoFraudCheck(long gpid)
                : base(gpid)
            {
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

        public SiftServiceActor(IRestClient restClient, ActorRef processBroker)
        {
            _restClient = restClient;
            _processBroker = processBroker;

            Receive<DoFraudCheck>(msg =>
                {
                    
                });
        }
    }
}
