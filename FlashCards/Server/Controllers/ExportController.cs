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
	public class ExportController : ControllerBase
	{
		readonly ILogger<ExportController> _logger;
		readonly ExportService _exportService;

		public ExportController(ILogger<ExportController> logger, ExportService exportService)
		{
			_logger = logger;
			_exportService = exportService;
		}

		[HttpPost("[action]")]
		[Authorize]
		public async Task<IActionResult> ExportDecks(ExportAnkiRequest request)
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
				var res = await _exportService.ExportAnkiPlainText(request);
				if (res.Success && res.Data != null)
					return File(res.Data, "text/*");
				else
					return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CardController:CreateCardSet({JsonSerializer.Serialize(request)})");
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
