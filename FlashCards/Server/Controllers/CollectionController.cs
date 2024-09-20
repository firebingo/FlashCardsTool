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
	public class CollectionController : ControllerBase
	{
		readonly ILogger<CollectionController> _logger;
		readonly CollectionService _collectionService;

		public CollectionController(ILogger<CollectionController> logger, CollectionService collectionService)
		{
			_logger = logger;
			_collectionService = collectionService;
		}

		[HttpPut("[action]")]
		[Authorize]
		public async Task<IActionResult> CreateCardCollection(CreateCardCollectionRequest request)
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
				var res = await _collectionService.CreateCardCollection(request);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CollectionController:CreateCardCollection({JsonSerializer.Serialize(request)})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpGet("[action]/{collectionId}")]
		[Authorize]
		public async Task<IActionResult> GetCardCollection([FromRoute] long collectionId)
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

				var res = await _collectionService.GetCardCollection(collectionId, userId);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CollectionController:GetCardCollection({collectionId})");
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
		public async Task<IActionResult> GetCardCollections()
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

				var res = await _collectionService.GetCardCollections(userId);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CollectionController:GetCardCollections()");
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
		public async Task<IActionResult> EditCardCollection(EditCardCollectionRequest request)
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

				var res = await _collectionService.EditCardCollection(request);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CollectionController:EditCardCollection({JsonSerializer.Serialize(request)})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpDelete("[action]/{collectionId}")]
		[Authorize]
		public async Task<IActionResult> DeleteCardCollection([FromRoute] long collectionId)
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

				var res = await _collectionService.DeleteCardCollection(collectionId, userId);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CollectionController:DeleteCardCollection({collectionId})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpPost("[action]")]
		[Authorize]
		public async Task<IActionResult> AddSetsToCollection([FromBody] AddSetsToCollectionRequest request)
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

				var res = await _collectionService.AddSetsToCollection(request);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CollectionController:AddSetsToCollection({JsonSerializer.Serialize(request)})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpPost("[action]")]
		[Authorize]
		public async Task<IActionResult> RemoveSetsFromCollection([FromBody] RemoveSetsFromCollectionRequest request)
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

				var res = await _collectionService.RemoveSetsFromCollection(request);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CollectionController:RemoveSetsFromCollection({JsonSerializer.Serialize(request)})");
				return StatusCode(500, new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.Unauthorized,
					Message = "EXCEPTION"
				});
			}
		}

		[HttpGet("[action]/{collectionId}")]
		[Authorize]
		public async Task<IActionResult> GetCardsForCollection([FromRoute] long collectionId)
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

				var res = await _collectionService.GetCardsForCollection(userId, collectionId);
				return StatusCode((int)res.StatusCode, res);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in CollectionController:RemoveSetsFromCollection({collectionId})");
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
