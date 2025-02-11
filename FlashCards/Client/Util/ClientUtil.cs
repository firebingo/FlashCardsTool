using FlashCards.Client.Models;
using FlashCards.Shared.Util;
using FlashCards.Shared.ViewModels;

namespace FlashCards.Client.Util
{
	public static class ClientUtil
	{
		public static string GetCardStyle(CardView card, UserSettingsView userSettings)
		{
			if (!userSettings.ColorCardPercent || card.CardSetExcludeStats || (card.PassCount + card.MissCount < userSettings.ColorCardThreshold))
				return string.Empty;

			var color = ColorUtil.ColorGradient(card.PassPercent, userSettings.GradColors);

			return $"border: 1px solid {color.ToHex()}";
		}

		public static string GetCardStyle(CardViewGame card, UserSettingsView userSettings)
		{
			if (!userSettings.ColorCardPercent || card.CardSetExcludeStats || (card.PassCount + card.MissCount < userSettings.ColorCardThreshold))
				return string.Empty;

			var color = ColorUtil.ColorGradient(card.PassPercent, userSettings.GradColors);

			return $"border: 1px solid {color.ToHex()}";
		}
	}
}
