using FlashCards.Shared.Util;
using System.Collections.Generic;

namespace FlashCards.Shared.ViewModels
{
	public class UserSettingsView
	{
		public bool ColorCardPercent { get; set; }
		public int ColorCardThreshold { get; set; }
		public bool ShowCardColorInGame { get; set; }
		public List<Color> GradColors { get; set; } = [];
	}
}
