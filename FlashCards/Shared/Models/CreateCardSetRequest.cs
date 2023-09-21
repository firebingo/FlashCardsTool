using System.Text.Json.Serialization;

namespace FlashCards.Shared.Models
{
	public class CreateCardSetRequest
	{
		public string Name { get; set; } = string.Empty;
		[JsonIgnore]
		public long UserId { get; set; }
	}
}
