using Nancy;

namespace LoyaltyService.API.HTTP.Modules
{
	public class BaseModule : NancyModule
	{
		public BaseModule()
		{
			Get["/"] = _ => {
				//do lbstatus / service monitoring in here
				return Response.AsText("OTWEB_ON - this value is hard coded!");
			};
		}
	}
}