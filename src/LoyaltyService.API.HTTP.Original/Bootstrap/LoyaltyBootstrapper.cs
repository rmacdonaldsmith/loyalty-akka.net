using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AggregateSource;
using LoyaltyService.API.HTTP.ErrorHandling;
using LoyaltyService.Common;
using LoyaltyService.Common.Configurations;
using MongoDB.Bson.Serialization;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using NEventStore;
using NEventStore.Persistence.MongoDB;
using NEventStore.Serialization;

namespace LoyaltyService.API.HTTP.Bootstrap
{
	public class LoyaltyBootstrapper : DefaultNancyBootstrapper
	{
		private const string RequestTimerKey = "RequestTimer";
		private const string RequestStartTimeKey = "RequestStartTime";
		private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("loyalty-service");

		protected override void ConfigureApplicationContainer (TinyIoCContainer container)
		{
			base.ConfigureApplicationContainer (container);
			Logger.Info ("Configuring the Nancy HTTP API...");

			RegisterObjects (container);
			RegisterClassMap();

			var appSettings = container.Resolve<IApplicationSettings>();
			var eventStore = Wireup.Init ()
				.UsingMongoPersistence (() => {
					var encryptedConnectionString = appSettings.Get ("ot.loyalty.mongo.encryptedconnectionstring");
					if (string.IsNullOrEmpty (encryptedConnectionString)) {
						return appSettings.Get ("ot.loyalty.mongo.connectionstring");
					}

					var dataDecryptor = container.Resolve<IEasyDataProtector>();
					var connectionString = dataDecryptor.Decrypt(encryptedConnectionString);
					return connectionString;
				}, new DocumentObjectSerializer (), new MongoPersistenceOptions ())
				.InitializeStorageEngine ()
				.UsingJsonSerialization ()
				.Compress ()
				.Build ();

			container
				.Register<IStoreEvents> (eventStore);



//			var giftClient = new OTRestClient (busInstance, appSettings.Get("ot.gift.base-url"));
//			var pointsVaultClient = new OTRestClient (busInstance, appSettings.Get ("ot.points-vault.base-url"));
//			var siftClient = new OTRestClient (busInstance, appSettings.Get("ot.sift.base-url"));
//			var userClient = new OTRestClient (busInstance, appSettings.Get("ot.user.base-url"));
//			var tmsClient = new OTRestClient (busInstance, appSettings.Get("ot.tms.base-url"));

		}

		protected override void RequestStartup (TinyIoCContainer container, IPipelines pipelines, NancyContext context)
		{
			//response time
			pipelines.BeforeRequest.AddItemToStartOfPipeline (ctx => {
				ctx.Items[RequestTimerKey] = Stopwatch.StartNew();
				ctx.Items[RequestStartTimeKey] = SystemTime.UtcNow;
				return null;
			});

			pipelines.AfterRequest.AddItemToEndOfPipeline ((ctx) => {
				var maybeStarted = MaybeGetItemFromContext<DateTime>(RequestStartTimeKey, ctx);
				var maybeTimer = MaybeGetItemFromContext<Stopwatch>(RequestTimerKey, ctx);

				if(maybeTimer.HasValue) {
					var stopwatch = maybeTimer.Value;
					stopwatch.Stop();
					ctx.Items.Remove(RequestTimerKey);

//					container
//						.Resolve<HitTracker>()
//						.AppendHit(ctx.ResolvedRoute.Description.Path, new Hit {
//							StartTime = maybeStarted.HasValue ? maybeStarted.Value : DateTime.MinValue,
//							IsError = ctx.Response.StatusCode >= HttpStatusCode.BadRequest,
//							TimeTaken = stopwatch.Elapsed
//						});
//
//					var message = new Logs.RequestLogEvent {
//						Method = ctx.Request.Method,
//						Url = ctx.Request.Url,
//						Status = (int)ctx.Response.StatusCode,
//						Duration = stopwatch.ElapsedTicks * 0.1
//					};

					container.Resolve<IBus>().Send (message);
				}
			});

			pipelines.OnError.AddItemToEndOfPipeline ((ctxt, ex) => {
				try {
					ex.Data["url"] = ctxt.Request.Url;
					var message = new Logs.ExceptionLogEvent(ex);

					container.Resolve<IBus> ().Send (message);
				} catch (Exception) {
				}

				return ErrorResponse.FromException(ex);
			});

			base.RequestStartup (container, pipelines, context);
		}

		void RegisterObjects (TinyIoCContainer container)
		{
//			var hitTracker = new HitTracker (HitTrackerSettings.Instance);
//			container.Register<HitTracker> (hitTracker);
//
//			var dataProtector = new EasyDataProtector (new DataDefender(), new EncryptionKey());
//			container.Register<IEasyDataProtector> (dataProtector);
		}

		private void RegisterClassMap()
		{
//			var type = typeof(IEvent);
//			var types = Assembly.GetAssembly(typeof(IEvent))
//				.GetTypes()
//				.Where(type.IsAssignableFrom)
//				.Where(t => t.IsClass);
//
//			foreach (var t in types)
//				BsonClassMap.LookupClassMap(t); 
		}

		private static Maybe<T> MaybeGetItemFromContext<T>(string key, NancyContext context)
		{
			object requestStart;
			if (context.Items.TryGetValue (key, out requestStart))
				return new Maybe<T> ((T)requestStart);

			return Maybe<T>.Empty;
		}
	}
}
