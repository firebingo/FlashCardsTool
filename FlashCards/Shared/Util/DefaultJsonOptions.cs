using System.Text.Json;

namespace FlashCards.Shared.Util
{
	public static class DefaultJsonOptions
	{
		public static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false
		};

		public static readonly JsonSerializerOptions CaseInsensitive = new JsonSerializerOptions()
		{
			PropertyNameCaseInsensitive = true,
			WriteIndented = false
		};
	}
}
