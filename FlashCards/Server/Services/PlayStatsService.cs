using FlashCards.Server.Data;
using FlashCards.Server.Data.Models;
using FlashCards.Shared.Models;
using FlashCards.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashCards.Server.Services
{
	public class PlayStatsService
	{
		readonly ILogger<PlayStatsService> _logger;
		readonly ServiceDbContext _dbContext;

		public PlayStatsService(ILogger<PlayStatsService> logger, ServiceDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		public async Task<StandardResponse> RecordPlay(RecordPlayRequest request)
		{
			var standardMessage = $"PlayStatsService:RecordPlay({JsonSerializer.Serialize(request)})";
			try
			{
				if (!request.SetId.HasValue && !request.CollectionId.HasValue)
				{
					return new StandardResponse()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.BadRequest,
						Message = "NO_SET_PROVIDED"
					};
				}

				var p = new PlayStats()
				{
					UserId = request.UserId,
					CollectionId = request.CollectionId.HasValue && request.CollectionId != -1 ? request.CollectionId : null,
					SetId = request.SetId,
					PlayTime = request.PlayTime,
					CreatedTime = DateTime.UtcNow,
					PassCount = 0,
					MissCount = 0
				};
				var cards = new List<Card>();
				if (request.SetId.HasValue)
				{
					var deck = (await _dbContext.CardSet.Include(x => x.Cards).Where(x => x.UserId == request.UserId && x.Id == request.SetId).ToListAsync()).FirstOrDefault();
					if (deck == null)
					{
						return new StandardResponse()
						{
							Success = false,
							StatusCode = System.Net.HttpStatusCode.NotFound,
							Message = "CARD_SET_NOT_FOUND"
						};
					}
					if (deck.Cards?.Count > 0)
						cards.AddRange(deck.Cards);
				}
				else
				{
					if (request.CollectionId == -1)
					{
						var decks = await _dbContext.CardSet.Include(x => x.Cards).Where(x => x.UserId == request.UserId).ToListAsync();
						foreach (var deck in decks)
						{
							if (deck.Cards?.Count > 0)
								cards.AddRange(deck.Cards);
						}
					}
					else
					{
						var collection = (await _dbContext.CardCollection
							.Include(x => x.CollectionSets)
							!.ThenInclude(x => x.CardSet)
							!.ThenInclude(x => x!.Cards)
							.Where(x => x.UserId == request.UserId && request.CollectionId == x.Id)
							.ToListAsync()).FirstOrDefault();
						if (collection == null)
						{
							return new StandardResponse()
							{
								Success = false,
								StatusCode = System.Net.HttpStatusCode.NotFound,
								Message = "COLLECTION_NOT_FOUND"
							};
						}
						foreach (var collectionSet in collection.CollectionSets!)
						{
							if (collectionSet.CardSet != null && collectionSet.CardSet.Cards != null && collectionSet.CardSet.Cards.Count > 0)
							{
								cards.AddRange(collectionSet.CardSet.Cards);
							}
						}
					}
				}
				foreach (var card in request.Cards)
				{
					var c = cards.FirstOrDefault(x => x.SetId == card.SetId && x.Id == card.CardId);
					if (c == null)
						continue;
					if (card.Pass)
					{
						p.PassCount++;
						c.PassCount++;
						c.LastPass = DateTime.UtcNow;
					}
					else
					{
						p.MissCount++;
						c.MissCount++;
						c.LastMiss = DateTime.UtcNow;
					}
				}
				_dbContext.PlayStats.Add(p);

				await _dbContext.SaveChangesAsync();
				return new StandardResponse();
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
	}
}
