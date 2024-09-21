using System.Text.Json.Serialization;

namespace FlashCards.Shared.Models
{
	public class EditCardCollectionRequest
	{
		public long CollectionId { get; set; }
		[JsonIgnore]
		public long UserId { get; set; }
		public string Name { get; set; } = string.Empty;
	}
}
