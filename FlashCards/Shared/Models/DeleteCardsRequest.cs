using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlashCards.Shared.Models
{
	public class DeleteCardsRequest
	{
		public long SetId { get; set; }
		[JsonIgnore]
		public long UserId { get; set; }
		public List<DeleteCardsCard> Cards { get; set; } = [];
	}

	public class DeleteCardsCard
	{
		public long Id { get; set; }
	}
}
