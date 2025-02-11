namespace FlashCards.Shared.Models.Auth
{
	public class UpdateUserSettingsRequest
	{
		public bool? ColorCardPercent { get; set; }
		public int? ColorCardThreshold { get; set; }
		public bool? ShowCardColorInGame { get; set; }
	}
}
