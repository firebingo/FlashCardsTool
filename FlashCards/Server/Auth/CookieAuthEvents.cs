using FlashCards.Server.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FlashCards.Server.Auth
{
	public class CookieAuthEvents : CookieAuthenticationEvents
	{
		readonly ServiceDbContext _dbContext;

		public CookieAuthEvents(ServiceDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
		{
			if (context.Request.Path.StartsWithSegments("/api") && context.Response.StatusCode == StatusCodes.Status200OK)
			{
				context.Response.Clear();
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return Task.CompletedTask;
			}
			context.Response.Redirect(context.RedirectUri);
			return Task.CompletedTask;
		}

		public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
		{
			if (context.Principal == null)
			{
				context.RejectPrincipal();
				await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			}
			if (!AuthHelper.GetUserIdFromContextUser(context.Principal!, out var userId))
			{
				context.RejectPrincipal();
				await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			}

			var user = (await _dbContext.Users.Where(x => x.Id == userId).ToListAsync()).FirstOrDefault();
			if (user == null || user.Disabled)
			{
				context.RejectPrincipal();

				await context.HttpContext.SignOutAsync(
					CookieAuthenticationDefaults.AuthenticationScheme);
			}
		}
	}
}
