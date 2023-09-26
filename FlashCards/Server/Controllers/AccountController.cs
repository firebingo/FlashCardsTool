using FlashCards.Server.Auth;
using FlashCards.Server.Services;
using FlashCards.Shared.Models;
using FlashCards.Shared.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FlashCards.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Produces("application/json")]
	public class AccountController : ControllerBase
	{
		readonly ILogger<AccountController> _logger;
		readonly UserManager _userManager;
		readonly AccountService _accountService;

		public AccountController(ILogger<AccountController> logger, UserManager userManager, AccountService accountService)
		{
			_logger = logger;
			_userManager = userManager;
			_accountService = accountService;
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> Login(LoginRequest request)
		{
			try
			{
				var res = await _userManager.SignIn(HttpContext, request);
				var statusCode = res ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Unauthorized;
				return StatusCode((int)statusCode, new StandardResponse()
				{
					Success = res,
					StatusCode = res ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Unauthorized
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in AccountController:Login({request?.UserName})");
				return Unauthorized(new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpPost("[action]")]
		[Authorize]
		public async Task<IActionResult> Logout()
		{
			try
			{
				await _userManager.SignOut(HttpContext);
				return Ok(new StandardResponse()
				{
					Success = true,
					StatusCode = System.Net.HttpStatusCode.OK
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in AccountController:Logout({HttpContext.User?.Identity?.Name})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpGet("[action]")]
		[Authorize]
		public IActionResult CheckUserAuth()
		{
			return Ok();
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> Register(RegisterRequest request)
		{
			try
			{
				var res = await _accountService.Register(request);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in AccountController:Logout({HttpContext.User?.Identity?.Name})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				});
			}
		}
	}
}
