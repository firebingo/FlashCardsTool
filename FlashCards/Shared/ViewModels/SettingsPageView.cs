namespace FlashCards.Shared.ViewModels
{
	public class SettingsPageView
	{
		public string Username { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string OldPassword { get; set; } = string.Empty;
		public string NewPassword { get; set; } = string.Empty;
		public string NewPasswordConfirm { get; set; } = string.Empty;

		public bool ColorCardPercent { get; set; }
		public int ColorCardThreshold { get; set; }
		public bool ShowCardColorInGame { get; set; }
	}
}
