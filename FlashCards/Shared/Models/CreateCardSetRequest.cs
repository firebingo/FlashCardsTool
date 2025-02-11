using System.Text.Json.Serialization;

namespace FlashCards.Shared.Models
{
	public class CreateCardSetRequest
	{
		public string Name { get; set; } = string.Empty;
		public bool? ExcludeFromStats { get; set; }
		[JsonIgnore]
		public long UserId { get; set; }
	}
}
