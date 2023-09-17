using FlashCards.Server.Configuration;
using FlashCards.Server.Data;
using FlashCards.Server.Models.Auth;
using FlashCards.Shared.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FlashCards.Server.Auth
{
	public class UserManager
	{
		readonly AppSettings _appSettings;
		readonly ServiceDbContext _dbContext;

		public UserManager(IOptions<AppSettings> appSettings, ServiceDbContext dbContext)
		{
			_dbContext = dbContext;
			_appSettings = appSettings.Value;
		}

		public async Task<bool> SignIn(HttpContext httpContext, LoginRequest user)
		{
			var dbUser = (await _dbContext.Users.Where(x => (x.UserName == user.UserName || x.Email == user.Email)).ToListAsync()).FirstOrDefault();

			if (dbUser == null)
			{
				await httpContext.ChallengeAsync();
				return false;
			}

			var passwordHash = HashUtil.HashPassword(user.Password, dbUser.Salt);
			if (passwordHash != dbUser.Password)
			{
				await httpContext.ChallengeAsync();
				return false;
			}

			ClaimsIdentity identity = new ClaimsIdentity(GetUserClaims(dbUser), CookieAuthenticationDefaults.AuthenticationScheme);
			ClaimsPrincipal principal = new ClaimsPrincipal(identity);

			await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
			return true;
		}

		public async Task SignOut(HttpContext httpContext)
		{
			await httpContext.SignOutAsync();
		}

		private static IEnumerable<Claim> GetUserClaims(User user)
		{
			List<Claim> claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.Email, user.Email)
			};
			claims.AddRange(GetUserRoleClaims(user));
			return claims;
		}

		private static IEnumerable<Claim> GetUserRoleClaims(User user)
		{
			List<Claim> claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			};
			//claims.Add(new Claim(ClaimTypes.Role, user.UserPermissionType.ToString()));
			return claims;
		}
	}
}
