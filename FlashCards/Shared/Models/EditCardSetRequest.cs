using System.Text.Json.Serialization;

namespace FlashCards.Shared.Models
{
	public class EditCardSetRequest
	{
		public long SetId { get; set; }
		[JsonIgnore]
		public long UserId { get; set; }
		public string Name { get; set; } = string.Empty;
	}
}
