using System.Net;

namespace FlashCards.Server.Models
{
	public class StandardResponse
	{
		public bool Success { get; set; } = true;
		public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
		public string Message { get; set; } = string.Empty;
		public string ReturnCode { get; set; } = string.Empty;
	}
}
