using FlashCards.Shared.ViewModels;
using System;

namespace FlashCards.Client.Models
{
	public class CardViewGame
	{
		public long Id { get; set; }
		public string? FrontValue { get; set; }
		public string? BackValue { get; set; }
		public bool Flipped { get; set; }
		public bool Correct { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public TimeSpan Time
		{
			get => EndTime - StartTime;
		}

		public CardViewGame()
		{

		}

		public CardViewGame(CardView card)
		{
			Id = card.Id;
			FrontValue = card.FrontValue;
			BackValue = card.BackValue;
		}
	}
}
