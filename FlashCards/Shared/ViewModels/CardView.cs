using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlashCards.Shared.ViewModels
{
	public class CardsView
	{
		public long? SetId { get; set; } = null;
		public string? SetName { get; set; } = null;
		public long? CollectionId { get; set; } = null;
		public string? CollectionName { get; set; } = null;
		public List<CardView> Cards { get; set; } = [];
	}

	public class CardView
	{
		public long Id { get; set; }
		public long SetId { get; set; }
		public string? FrontValue { get; set; }
		public string? BackValue { get; set; }
		public DateTime ModifiedTime { get; set; }
		[JsonIgnore]
		public bool Flipped { get; set; }
		[JsonIgnore]
		public bool DeleteConfirm { get; set; }
		[JsonIgnore]
		public bool Loading { get; set; }
	}
}
