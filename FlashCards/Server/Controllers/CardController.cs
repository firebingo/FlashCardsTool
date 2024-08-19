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
	public class CardController : ControllerBase
	{
		readonly ILogger<AccountController> _logger;
		readonly CardService _cardService;

		public CardController(ILogger<AccountController> logger, CardService cardService)
		{
			_logger = logger;
			_cardService = cardService;
		}

		[HttpPut("[action]")]
		[Authorize]
		public async Task<IActionResult> CreateCardSet(CreateCardSetRequest request)
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
				var res = await _cardService.CreateCardSet(request);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CardController:CreateCardSet({JsonSerializer.Serialize(request)})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpGet("[action]/{setId}")]
		[Authorize]
		public async Task<IActionResult> GetCardSet([FromRoute] long setId)
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

				var res = await _cardService.GetCardSet(setId, userId);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CardController:GetCardSet({setId})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpGet("[action]")]
		[Authorize]
		public async Task<IActionResult> GetCardSets()
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

				var res = await _cardService.GetCardSets(userId);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CardController:GetCardSets()");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpPatch("[action]")]
		[Authorize]
		public async Task<IActionResult> EditCardSet(EditCardSetRequest request)
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

				var res = await _cardService.EditCardSet(request);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CardController:EditCardSet({JsonSerializer.Serialize(request)})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpDelete("[action]/{setId}")]
		[Authorize]
		public async Task<IActionResult> DeleteCardSet([FromRoute] long setId)
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

				var res = await _cardService.DeleteCardSet(setId, userId);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CardController:DeleteCardSet({setId})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpPut("[action]")]
		[Authorize]
		public async Task<IActionResult> CreateCards(CreateCardsRequest request)
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
				var res = await _cardService.CreateCards(request);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CardController:CreateCards({JsonSerializer.Serialize(request)})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpGet("[action]/{setId}")]
		[Authorize]
		public async Task<IActionResult> GetCardsForSet([FromRoute] long setId)
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

				var res = await _cardService.GetCardsForSet(userId, setId);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CardController:GetCardsForSet({setId})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpPatch("[action]")]
		[Authorize]
		public async Task<IActionResult> EditCards(EditCardsRequest request)
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
				var res = await _cardService.EditCards(request);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CardController:EditCards({JsonSerializer.Serialize(request)})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpDelete("[action]")]
		[Authorize]
		public async Task<IActionResult> DeleteCards(DeleteCardsRequest request)
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
				var res = await _cardService.DeleteCards(request);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CardController:DeleteCards({JsonSerializer.Serialize(request)})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}
	}
}
