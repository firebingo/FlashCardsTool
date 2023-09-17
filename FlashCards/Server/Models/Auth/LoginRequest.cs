namespace FlashCards.Server.Models.Auth
{
	public class LoginRequest
	{
		public string? UserName { get; set; }
		public string? Email { get; set; }
		public string Password { get; set; } = string.Empty;
	}
}
