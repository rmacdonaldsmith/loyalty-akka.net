using System;
using OpenTable.Services.Components.Monitoring.Monitors.HitTracker;
using LoyaltyService.API.HTTP.Monitoring;
using LoyaltyService.API.HTTP.Models;
using Nancy;

namespace LoyaltyService.API.HTTP.Modules
{
	public class ServiceStatusModule : BaseModule
	{
		public ServiceStatusModule (HitTracker hitTracker)
		{
			Get ["/service-status"] = parameters => {
				var monitorFactory = new MonitorFactory(hitTracker);
				var monitors = monitorFactory.Build();

				return Response.AsJson(monitors);
			};
		}
	}
}

