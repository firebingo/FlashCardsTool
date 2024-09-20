using System;
using System.Collections.Generic;

namespace FlashCards.Shared.ViewModels
{
	public class CardCollectionView
	{
		public long Id { get; set; }
		public string CollectionName { get; set; } = string.Empty;
		public List<long> DeckIds { get; set; } = new List<long>();
		public int CardCount { get; set; }
		public DateTime ModifiedTime { get; set; } = DateTime.MinValue;
	}
}
