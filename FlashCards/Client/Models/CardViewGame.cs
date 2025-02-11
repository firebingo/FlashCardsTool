using FlashCards.Shared.ViewModels;
using System;

namespace FlashCards.Client.Models
{
	public class CardViewGame
	{
		public long Id { get; set; }
		public long SetId { get; set; }
		public string? FrontValue { get; set; }
		public string? BackValue { get; set; }
		public bool Flipped { get; set; }
		public bool Correct { get; set; }
		public int PassCount { get; set; }
		public int MissCount { get; set; }
		public DateTime? LastMiss { get; set; }
		public DateTime? LastPass { get; set; }
		public bool CardSetExcludeStats { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public TimeSpan Time
		{
			get => EndTime - StartTime;
		}
		public float PassPercent { get => (MissCount == 0 && PassCount == 0) ? 1.0f : ((float)PassCount / (PassCount + MissCount)); }
		public DateTime LastSeenTime
		{
			get
			{
				if (!LastMiss.HasValue && !LastPass.HasValue)
					return DateTime.MinValue;
				else if (LastMiss.HasValue && !LastPass.HasValue)
					return LastMiss.Value;
				else if (!LastMiss.HasValue && LastPass.HasValue)
					return LastPass.Value;
				else
					return LastPass > LastMiss ? LastPass.Value : LastMiss!.Value;
			}
		}

		public CardViewGame()
		{

		}

		public CardViewGame(CardView card)
		{
			Id = card.Id;
			SetId = card.SetId;
			FrontValue = card.FrontValue;
			BackValue = card.BackValue;
			PassCount = card.PassCount;
			MissCount = card.MissCount;
			CardSetExcludeStats = card.CardSetExcludeStats;
			LastMiss = card.LastMiss;
			LastPass = card.LastPass;
		}
	}
}
