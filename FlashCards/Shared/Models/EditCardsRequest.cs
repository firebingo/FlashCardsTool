using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlashCards.Shared.Models
{
	public class EditCardsRequest
	{
		public long SetId { get; set; }
		[JsonIgnore]
		public long UserId { get; set; }
		public List<EditCardsCard> Cards { get; set; } = new List<EditCardsCard>();
	}

	public class EditCardsCard
	{
		public long Id { get; set; }
		public string? FrontValue { get; set; } = string.Empty;
		public string? BackValue { get; set; } = string.Empty;
	}
}
