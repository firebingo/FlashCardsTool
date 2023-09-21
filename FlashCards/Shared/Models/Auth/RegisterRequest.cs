namespace FlashCards.Shared.Models.Auth
{
	public class RegisterRequest
	{
		public string UserName { get; set; } = string.Empty;
		public string? Email { get; set; } = null;
		public string Password { get; set; } = string.Empty;
	}
}
