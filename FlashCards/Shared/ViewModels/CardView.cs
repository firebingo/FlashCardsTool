using System;
using System.Collections.Generic;

namespace FlashCards.Shared.ViewModels
{
	public class CardsView
	{
		public List<CardView> Cards { get; set; } = new List<CardView>();
	}

	public class CardView
	{
		public long Id { get; set; }
		public string? FrontValue { get; set; }
		public string? BackValue { get; set; }
		public DateTime ModifiedTime { get; set; }
	}
}
