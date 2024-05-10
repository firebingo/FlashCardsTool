using FlashCards.Server.Data;
using FlashCards.Server.Data.Models;
using FlashCards.Shared.Models.Auth;
using FlashCards.Shared.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FlashCards.Server.Auth
{
	public class UserManager
	{
		readonly ServiceDbContext _dbContext;

		public UserManager(ServiceDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<bool> SignIn(HttpContext httpContext, LoginRequest user)
		{
			var dbUser = (await _dbContext.Users.Where(x => (x.UserName == user.UserName || x.Email == user.Email)).ToListAsync()).FirstOrDefault();

			if (dbUser?.Disabled ?? true)
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

			await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties()
			{
				ExpiresUtc = DateTime.UtcNow.AddDays(3),
				IsPersistent = true,
				AllowRefresh = true
			});
			return true;
		}

		public async Task SignOut(HttpContext httpContext)
		{
			await httpContext.SignOutAsync();
		}

		private static List<Claim> GetUserClaims(User user)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
			};
			claims.AddRange(GetUserRoleClaims(user));
			return claims;
		}

		private static List<Claim> GetUserRoleClaims(User user)
		{
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			};
			//claims.Add(new Claim(ClaimTypes.Role, user.UserPermissionType.ToString()));
			return claims;
		}
	}
}
