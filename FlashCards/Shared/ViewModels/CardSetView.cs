﻿using System;

namespace FlashCards.Shared.ViewModels
{
	public class CardSetView
	{
		public long Id { get; set; }
		public string SetName { get; set; } = string.Empty;
		public int CardCount { get; set; } = 0;
		public DateTime ModifiedTime { get; set; } = DateTime.MinValue;
	}
}
