using FlashCards.Server.Data;
using FlashCards.Server.Data.Models;
using FlashCards.Shared.Models;
using FlashCards.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashCards.Server.Services
{
	public class CardService
	{
		readonly ILogger<CardService> _logger;
		readonly ServiceDbContext _dbContext;

		public CardService(ILogger<CardService> logger, ServiceDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		public async Task<StandardResponse<CardSetView>> CreateCardSet(CreateCardSetRequest request)
		{
			var standardMessage = $"CardService:CreateCardSet({JsonSerializer.Serialize(request)})";
			try
			{
				if (string.IsNullOrWhiteSpace(request?.Name?.Trim()))
				{
					return new StandardResponse<CardSetView>()
					{
						StatusCode = System.Net.HttpStatusCode.BadRequest,
						Message = "CARD_SET_EMPTY_NAME",
						Success = false
					};
				}
				request.Name = request.Name.Trim();
				if (request.Name.Length > 127)
					request.Name = request.Name[0..127];

				var set = await _dbContext.CardSet.AddAsync(new CardSet()
				{
					Id = 0,
					SetName = request.Name,
					ModifiedTime = DateTime.UtcNow,
					UserId = request.UserId,
					CreatedTime = DateTime.UtcNow
				});

				await _dbContext.SaveChangesAsync();
				return new StandardResponse<CardSetView>()
				{
					Data = new CardSetView()
					{
						Id = set.Entity.Id,
						SetName = set.Entity.SetName,
						ModifiedTime = set.Entity.ModifiedTime
					}
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<CardSetView>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse<CardSetView>> GetCardSet(long setId, long userId)
		{
			var standardMessage = $"CardService:GetCardSets({setId}, {userId})";
			try
			{
				var set = (await _dbContext.CardSet.Where(x => x.UserId == userId && x.Id == setId).ToListAsync())?.FirstOrDefault();
				if (set == null)
				{
					return new StandardResponse<CardSetView>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "CARD_SET_NOT_FOUND"
					};
				}

				var setView = new CardSetView()
				{
					Id = set.Id,
					ModifiedTime = set.ModifiedTime,
					CardCount = await _dbContext.Card.CountAsync(y => y.SetId == set.Id),
					SetName = set.SetName
				};

				return new StandardResponse<CardSetView>()
				{
					Data = setView
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<CardSetView>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse<List<CardSetView>>> GetCardSets(long userId)
		{
			var standardMessage = $"CardService:GetCardSets({userId})";
			try
			{
				var sets = await _dbContext.CardSet.Where(x => x.UserId == userId).ToListAsync();
				var viewSets = new List<CardSetView>();
				foreach (var set in sets)
				{
					viewSets.Add(new CardSetView()
					{
						Id = set.Id,
						ModifiedTime = set.ModifiedTime,
						CardCount = await _dbContext.Card.CountAsync(y => y.SetId == set.Id),
						SetName = set.SetName
					});
				}

				return new StandardResponse<List<CardSetView>>()
				{
					Data = viewSets
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<List<CardSetView>>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse> EditCardSet(EditCardSetRequest request)
		{
			var standardMessage = $"CardService:GetCardSets({JsonSerializer.Serialize(request)})";
			try
			{
				var set = (await _dbContext.CardSet.Where(x => x.Id == request.SetId && x.UserId == request.UserId).ToListAsync()).FirstOrDefault();
				if (set == null)
				{
					return new StandardResponse()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "CARD_SET_NOT_FOUND"
					};
				}

				request.Name = request.Name.Trim();
				if (request.Name.Length > 127)
					request.Name = request.Name[0..127];
				set.SetName = request.Name;
				set.ModifiedTime = DateTime.UtcNow;

				await _dbContext.SaveChangesAsync();

				return new StandardResponse();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse> DeleteCardSet(long setId, long userId)
		{
			var standardMessage = $"CardService:DeleteCardSet({setId}, {userId})";
			try
			{
				var set = (await _dbContext.CardSet.Where(x => x.Id == setId && x.UserId == userId).ToListAsync()).FirstOrDefault();
				if (set == null)
				{
					return new StandardResponse<CardsView>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "CARD_SET_NOT_FOUND"
					};
				}

				_dbContext.CardSet.Remove(set);
				await _dbContext.SaveChangesAsync();

				return new StandardResponse();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse<CardsView>> CreateCards(CreateCardsRequest request)
		{
			var standardMessage = $"CardService:CreateCards({JsonSerializer.Serialize(request)})";
			try
			{
				var cardSet = (await _dbContext.CardSet.Where(x => x.Id == request.SetId && x.UserId == request.UserId).ToListAsync()).FirstOrDefault();
				if (cardSet == null)
				{
					return new StandardResponse<CardsView>()
					{
						StatusCode = System.Net.HttpStatusCode.BadRequest,
						Message = "CARD_SET_NOT_EXIST",
						Success = false
					};
				}

				var newCards = new List<Card>();
				foreach (var card in request.Cards)
				{
					card.BackValue = card.BackValue?.Trim();
					card.FrontValue = card.FrontValue?.Trim();
					if (card.BackValue?.Length >= 65535)
						card.BackValue = card.BackValue[0..65534];
					if (card.FrontValue?.Length >= 65535)
						card.FrontValue = card.FrontValue[0..65534];

					newCards.Add(new Card()
					{
						Id = 0,
						FrontValue = card.FrontValue,
						BackValue = card.BackValue,
						SetId = request.SetId,
						CreatedTime = DateTime.UtcNow,
						ModifiedTime = DateTime.UtcNow
					});
				}

				await _dbContext.Card.AddRangeAsync(newCards);
				await _dbContext.SaveChangesAsync();
				var ret = new CardsView()
				{
					SetId = cardSet.Id,
					SetName = cardSet.SetName,
					Cards = newCards
						.Select(x => new CardView()
						{
							Id = x.Id,
							SetId = cardSet.Id,
							BackValue = x.BackValue,
							FrontValue = x.FrontValue,
							ModifiedTime = x.ModifiedTime
						}).ToList()
				};

				return new StandardResponse<CardsView>()
				{
					Data = ret
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<CardsView>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse<CardsView>> GetCardsForSet(long userId, long setId)
		{
			var standardMessage = $"CardService:GetCardsForSet({userId}, {setId})";
			try
			{
				var set = (await _dbContext.CardSet.Include(x => x.Cards).Where(x => x.Id == setId && x.UserId == userId).ToListAsync()).FirstOrDefault();
				if (set?.Cards == null)
				{
					return new StandardResponse<CardsView>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "CARD_SET_NOT_FOUND"
					};
				}
				return new StandardResponse<CardsView>()
				{
					Data = new CardsView()
					{
						SetId = set.Id,
						SetName = set.SetName,
						Cards = set.Cards.Select(x => new CardView()
						{
							Id = x.Id,
							SetId = set.Id,
							BackValue = x.BackValue,
							FrontValue = x.FrontValue,
							ModifiedTime = x.ModifiedTime
						}).ToList()
					}
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<CardsView>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse<CardsView>> EditCards(EditCardsRequest request)
		{
			var standardMessage = $"CardService:EditCards({JsonSerializer.Serialize(request)})";
			try
			{
				var set = (await _dbContext.CardSet.Include(x => x.Cards).Where(x => x.Id == request.SetId && x.UserId == request.UserId).ToListAsync()).FirstOrDefault();
				if (set?.Cards == null)
				{
					return new StandardResponse<CardsView>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "CARD_SET_NOT_FOUND"
					};
				}
				var retCards = new List<CardView>();
				foreach (var card in request.Cards)
				{
					var dbCard = set.Cards.FirstOrDefault(x => x.Id == card.Id);
					if (dbCard == null)
					{
						return new StandardResponse<CardsView>()
						{
							Success = false,
							StatusCode = System.Net.HttpStatusCode.NotFound,
							Message = "CARD_NOT_FOUND"
						};
					}
					dbCard.FrontValue = card.FrontValue;
					dbCard.BackValue = card.BackValue;
					dbCard.ModifiedTime = DateTime.UtcNow;
					retCards.Add(new CardView()
					{
						Id = dbCard.Id,
						SetId = set.Id,
						FrontValue = dbCard.FrontValue,
						BackValue = dbCard.BackValue,
						ModifiedTime = dbCard.ModifiedTime,
					});
				}

				await _dbContext.SaveChangesAsync();
				return new StandardResponse<CardsView>()
				{
					Data = new CardsView()
					{
						SetId = set.Id,
						SetName = set.SetName,
						Cards = retCards
					}
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<CardsView>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse> DeleteCards(DeleteCardsRequest request)
		{
			var standardMessage = $"CardService:DeleteCards({JsonSerializer.Serialize(request)})";
			try
			{
				var set = (await _dbContext.CardSet.Include(x => x.Cards).Where(x => x.Id == request.SetId && x.UserId == request.UserId).ToListAsync()).FirstOrDefault();
				if (set?.Cards == null)
				{
					return new StandardResponse()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "CARD_SET_NOT_FOUND"
					};
				}
				var deleteCards = set.Cards.IntersectBy(request.Cards.Select(x => x.Id), x => x.Id);
				_dbContext.RemoveRange(deleteCards);
				await _dbContext.SaveChangesAsync();
				return new StandardResponse();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}
	}
}
