using System;
using LoyaltyService.Common;
using Nancy;
using Nancy.Responses;
using Nancy.Serialization.JsonNet;
using Newtonsoft.Json;

namespace LoyaltyService.API.HTTP.ErrorHandling
{
	// Credit Due: based this article http://paulstovell.com/blog/consistent-error-handling-with-nancy

	public class ErrorResponse : JsonResponse
	{
		private readonly Error error;

		private ErrorResponse(Error error) 
			: base(error, new JsonNetSerializer())
		{
			Ensure.NotNull(error, "error");
			this.error = error;
		}

		public string ErrorMessage { get { return error.ErrorMessage; } }
		public string FullException { get { return error.FullException; } }
		public string[] Errors { get { return error.Errors; } }

		public static ErrorResponse FromMessage(string message)
		{
			return new ErrorResponse(new Error { ErrorMessage = message });
		}

		public static ErrorResponse FromException(Exception ex)
		{
			var error = new Error { ErrorMessage = ex.Message, FullException = ex.ToString() };
			var response = new ErrorResponse(error);
			response.StatusCode = HttpStatusCode.InternalServerError;
			return response;
		}

		class Error
		{
			public string ErrorMessage { get; set; }

			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public string FullException { get; set; }

			[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
			public string[] Errors { get; set; }
		}
	}
}

