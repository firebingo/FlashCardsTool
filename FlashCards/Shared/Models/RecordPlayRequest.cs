using System;
using System.Collections.Generic;

namespace FlashCards.Shared.Models
{
	public class RecordPlayRequest
	{
		public long? SetId { get; set; }
		public long? CollectionId { get; set; }
		public long UserId { get; set; }
		public TimeSpan PlayTime { get; set; }
		public List<RecordPlayRequestCard> Cards { get; set; } = [];
	}

	public class RecordPlayRequestCard
	{
		public long SetId { get; set; }
		public long CardId { get; set; }
		public bool Pass { get; set; }
	}
}
