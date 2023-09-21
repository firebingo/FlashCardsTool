using System.Net;

namespace FlashCards.Shared.Models
{
	public class StandardResponse
	{
		public bool Success { get; set; } = true;
		public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
		public string Message { get; set; } = string.Empty;
		public string ReturnCode { get; set; } = string.Empty;
	}

	public class StandardResponse<T> : StandardResponse
	{
		public T? Data { get; set; }
	}
}
