using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlashCards.Shared.Models
{
	public class CreateCardsRequest
	{
		public long SetId { get; set; }
		[JsonIgnore]
		public long UserId { get; set; }
		public List<CreateCardsCard> Cards { get; set; } = [];
	}

	public class CreateCardsCard
	{
		public string? FrontValue { get; set; } = string.Empty;
		public string? BackValue { get; set; } = string.Empty;
	}
}
