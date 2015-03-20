using Nancy;
using Nancy.TinyIoc;
using RestSharp;

namespace LoyaltyService.API.HTTP.IntegrationTests
{
	public class TestingBootstrapper : DefaultNancyBootstrapper
	{
		protected IRestClient PointsVaultServiceClient { get; set; }
		protected IRestClient UserServiceClient { get; set; }
	    protected IRestClient SiftServiceClient { get; set; }
	    protected IRestClient GiftServiceClient { get; set; }
		protected IRestClient TmsServiceClient { get; set; }

	    protected override void ConfigureApplicationContainer (TinyIoCContainer container)
		{
			base.ConfigureApplicationContainer (container);

		}

		public TestingBootstrapper WithPointsVaultServiceClient(IRestClient restClient)
		{
			PointsVaultServiceClient = restClient;
			return this;
		}

		public TestingBootstrapper WithUserServiceClient(IRestClient restClient)
		{
			this.UserServiceClient = restClient;
			return this;
		}

	    public TestingBootstrapper WithSiftServiceClient(IRestClient restClient)
	    {
	        this.SiftServiceClient = restClient;
	        return this;
	    }

        public TestingBootstrapper WithGiftServiceClient(IRestClient restClient)
        {
            this.GiftServiceClient = restClient;
            return this;
        }

		public TestingBootstrapper WithTransactionMessageClient(IRestClient restClient)
		{
			this.TmsServiceClient = restClient;
			return this;
		}
	}
}

