using System.Linq;
using System.Security.Claims;

namespace FlashCards.Server.Auth
{
	public static class AuthHelper
	{
		public static bool GetUserIdFromContextUser(ClaimsPrincipal principal, out long userId)
		{
			var userIdS = principal.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
			if (!long.TryParse(userIdS, out userId))
				return false;
			return true;
		}
	}
}
