using FlashCards.Server.Auth;
using FlashCards.Server.Services;
using FlashCards.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashCards.Server.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Produces("application/json")]
	public class PlayStatsController : ControllerBase
	{
		readonly ILogger<PlayStatsController> _logger;
		readonly PlayStatsService _playStatsService;

		public PlayStatsController(ILogger<PlayStatsController> logger, PlayStatsService playStatsService)
		{
			_logger = logger;
			_playStatsService = playStatsService;
		}

		[HttpPut("[action]")]
		[Authorize]
		public async Task<IActionResult> RecordPlay(RecordPlayRequest request)
		{
			try
			{
				if (!AuthHelper.GetUserIdFromContextUser(HttpContext.User, out var userId))
				{
					return Unauthorized(new StandardResponse()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.Unauthorized,
						Message = "UNAUTHORIZED"
					});
				}

				request.UserId = userId;
				var res = await _playStatsService.RecordPlay(request);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in PlayStatsController:RecordPlay({JsonSerializer.Serialize(request)})");
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
