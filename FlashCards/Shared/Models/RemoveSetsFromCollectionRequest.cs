using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlashCards.Shared.Models
{
	public class RemoveSetsFromCollectionRequest
	{
		public long CollectionId { get; set; }
		public List<long> SetIds { get; set; } = [];
		[JsonIgnore]
		public long UserId { get; set; }
	}
}
