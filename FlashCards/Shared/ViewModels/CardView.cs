using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FlashCards.Shared.ViewModels
{
	public class CardsView
	{
		public long SetId { get; set; }
		public string SetName { get; set; } = string.Empty;
		public List<CardView> Cards { get; set; } = new List<CardView>();
	}

	public class CardView
	{
		public long Id { get; set; }
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
