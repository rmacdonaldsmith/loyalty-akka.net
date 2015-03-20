using Nancy;
using Nancy.Testing;
using NSubstitute;
using NUnit.Framework;
using RestSharp;

namespace LoyaltyService.API.HTTP.IntegrationTests
{
	[TestFixture]
	public abstract class ServiceSpec<TModule> where TModule : NancyModule
	{
		protected Browser ServiceInstance;
		protected IRestClient PointsVaultServiceClient = Substitute.For<IRestClient> ();
		protected IRestClient UserServiceClient = Substitute.For<IRestClient> ();
		protected IRestClient GiftServiceClient = Substitute.For<IRestClient> ();
		protected IRestClient SiftClient = Substitute.For<IRestClient> ();
		protected IRestClient TmsClient = Substitute.For<IRestClient> ();

		[TestFixtureSetUp]
		public void BeforeAll()
		{
			StaticConfiguration.DisableErrorTraces = false; //so that we can see stack trace, etc in the response if there is an exception
			ServiceInstance = new Browser (
				new TestingBootstrapper()
					.WithUserServiceClient (UserServiceClient)
					.WithGiftServiceClient (GiftServiceClient)
					.WithSiftServiceClient(SiftClient)
					.WithPointsVaultServiceClient(PointsVaultServiceClient)
					.WithTransactionMessageClient(TmsClient)
			);
		}
	}
}
