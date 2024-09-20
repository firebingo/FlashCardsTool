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
	public class CollectionService
	{
		readonly ILogger<CollectionService> _logger;
		readonly ServiceDbContext _dbContext;

		public CollectionService(ILogger<CollectionService> logger, ServiceDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		public async Task<StandardResponse<CardCollectionView>> CreateCardCollection(CreateCardCollectionRequest request)
		{
			var standardMessage = $"CollectionService:CreateCardCollection({JsonSerializer.Serialize(request)})";
			try
			{
				if (string.IsNullOrWhiteSpace(request?.Name?.Trim()))
				{
					return new StandardResponse<CardCollectionView>()
					{
						StatusCode = System.Net.HttpStatusCode.BadRequest,
						Message = "COLLECTION_EMPTY_NAME",
						Success = false
					};
				}
				request.Name = request.Name.Trim();
				if (request.Name.Length > 127)
					request.Name = request.Name[0..127];

				var set = await _dbContext.CardCollection.AddAsync(new CardSetCollection()
				{
					Id = 0,
					CollectionName = request.Name,
					ModifiedTime = DateTime.UtcNow,
					UserId = request.UserId,
					CreatedTime = DateTime.UtcNow
				});

				await _dbContext.SaveChangesAsync();
				return new StandardResponse<CardCollectionView>()
				{
					Data = new CardCollectionView()
					{
						Id = set.Entity.Id,
						CollectionName = set.Entity.CollectionName,
						ModifiedTime = set.Entity.ModifiedTime
					}
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<CardCollectionView>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse<CardCollectionView>> GetCardCollection(long collectionId, long userId)
		{
			var standardMessage = $"CollectionService:GetCardCollection({collectionId}, {userId})";
			try
			{
				var set = (await _dbContext.CardCollection.Where(x => x.UserId == userId && x.Id == collectionId).ToListAsync())?.FirstOrDefault();
				if (set == null)
				{
					return new StandardResponse<CardCollectionView>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "COLLECTION_NOT_FOUND"
					};
				}

				var decks = await _dbContext.CardCollectionSets.Where(x => x.CollectionId == set.Id).ToListAsync();
				var collectionView = new CardCollectionView()
				{
					Id = set.Id,
					ModifiedTime = set.ModifiedTime,
					CollectionName = set.CollectionName,
					DeckIds = decks.Select(x => x.SetId).ToList()
				};

				return new StandardResponse<CardCollectionView>()
				{
					Data = collectionView
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<CardCollectionView>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse<List<CardCollectionView>>> GetCardCollections(long userId)
		{
			var standardMessage = $"CollectionService:GetCardCollections({userId})";
			try
			{
				var sets = await _dbContext.CardCollection.Where(x => x.UserId == userId).ToListAsync();
				var viewSets = new List<CardCollectionView>();
				foreach (var set in sets)
				{
					var decks = await _dbContext.CardCollectionSets.Where(x => x.CollectionId == set.Id).ToListAsync();
					viewSets.Add(new CardCollectionView()
					{
						Id = set.Id,
						ModifiedTime = set.ModifiedTime,
						CollectionName = set.CollectionName,
						DeckIds = decks.Select(x => x.SetId).ToList()
					});
				}

				return new StandardResponse<List<CardCollectionView>>()
				{
					Data = viewSets
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<List<CardCollectionView>>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse> EditCardCollection(EditCardCollectionRequest request)
		{
			var standardMessage = $"CollectionService:EditCardCollection({JsonSerializer.Serialize(request)})";
			try
			{
				var collection = (await _dbContext.CardCollection.Where(x => x.Id == request.CollectionId && x.UserId == request.UserId).ToListAsync()).FirstOrDefault();
				if (collection == null)
				{
					return new StandardResponse()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "COLLECTION_NOT_FOUND"
					};
				}

				request.Name = request.Name.Trim();
				if (request.Name.Length > 127)
					request.Name = request.Name[0..127];
				collection.CollectionName = request.Name;
				collection.ModifiedTime = DateTime.UtcNow;

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

		public async Task<StandardResponse> DeleteCardCollection(long collectionId, long userId)
		{
			var standardMessage = $"CollectionService:DeleteCardCollection({collectionId}, {userId})";
			try
			{
				var collection = (await _dbContext.CardCollection.Where(x => x.Id == collectionId && x.UserId == userId).ToListAsync()).FirstOrDefault();
				if (collection == null)
				{
					return new StandardResponse<CardsView>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "COLLECTION_NOT_FOUND"
					};
				}

				_dbContext.CardCollection.Remove(collection);
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

		public async Task<StandardResponse<List<long>>> AddSetsToCollection(AddSetsToCollectionRequest request)
		{
			var standardMessage = $"CollectionService:AddSetsToCollection({JsonSerializer.Serialize(request)})";
			try
			{
				var collection = (await _dbContext.CardCollection.Where(x => x.Id == request.CollectionId && x.UserId == request.UserId).ToListAsync()).FirstOrDefault();
				if (collection == null)
				{
					return new StandardResponse<List<long>>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "COLLECTION_NOT_FOUND"
					};
				}

				var cardSets = await _dbContext.CardSet.Where(x => request.SetIds.Contains(x.Id)).ToListAsync();
				if (cardSets.Count == 0)
				{
					return new StandardResponse<List<long>>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "NO_CARD_SETS_FOUND"
					};
				}
				var newCollectionSets = new List<CardSetCollectionSets>();
				foreach (var set in cardSets)
				{
					var collectionSet = (await _dbContext.CardCollectionSets.Where(x => x.CollectionId == collection.Id && x.SetId == set.Id).ToListAsync()).FirstOrDefault();
					if (collectionSet == null)
					{
						newCollectionSets.Add(new CardSetCollectionSets()
						{
							CollectionId = collection.Id,
							SetId = set.Id,
							CreatedTime = DateTime.UtcNow,
							ModifiedTime = DateTime.UtcNow
						});
					}
				}
				if (newCollectionSets.Count != 0)
				{
					_dbContext.CardCollectionSets.AddRange(newCollectionSets);
					await _dbContext.SaveChangesAsync();
				}

				return new StandardResponse<List<long>>()
				{
					Data = newCollectionSets.Select(x => x.SetId).ToList()
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<List<long>>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse<List<long>>> RemoveSetsFromCollection(RemoveSetsFromCollectionRequest request)
		{
			var standardMessage = $"CollectionService:RemoveSetsFromCollection({JsonSerializer.Serialize(request)})";
			try
			{
				var collection = (await _dbContext.CardCollection.Where(x => x.Id == request.CollectionId && x.UserId == request.UserId).ToListAsync()).FirstOrDefault();
				if (collection == null)
				{
					return new StandardResponse<List<long>>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "COLLECTION_NOT_FOUND"
					};
				}

				var cardSets = await _dbContext.CardSet.Where(x => request.SetIds.Contains(x.Id)).ToListAsync();
				if (cardSets.Count == 0)
				{
					return new StandardResponse<List<long>>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "NO_CARD_SETS_FOUND"
					};
				}

				var checkSets = cardSets.Select(x => x.Id).ToArray();
				var collectionSets = (await _dbContext.CardCollectionSets.Where(x => x.CollectionId == collection.Id && checkSets.Contains(x.SetId)).ToListAsync());
				if (collectionSets.Count != 0)
				{
					_dbContext.CardCollectionSets.RemoveRange(collectionSets);
					await _dbContext.SaveChangesAsync();
				}

				return new StandardResponse<List<long>>()
				{
					Data = collectionSets.Select(x => x.SetId).ToList()
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<List<long>>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}

		public async Task<StandardResponse<CardsView>> GetCardsForCollection(long userId, long collectionId)
		{
			var standardMessage = $"CollectionService:GetCardsForCollection({userId}, {collectionId})";
			try
			{
				var collection = (await _dbContext.CardCollection
					.Include(x => x.CollectionSets)
					!.ThenInclude(x => x.CardSet)
					!.ThenInclude(x => x!.Cards)
					.Where(x => x.UserId == userId && collectionId == x.Id)
					.ToListAsync()).FirstOrDefault();
				if (collection == null)
				{
					return new StandardResponse<CardsView>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.NotFound,
						Message = "COLLECTION_NOT_FOUND"
					};
				}
				if (collection.CollectionSets == null || collection.CollectionSets.Count == 0)
				{
					return new StandardResponse<CardsView>();
				}

				var cards = new List<CardView>();
				foreach (var collectionSet in collection.CollectionSets)
				{
					if (collectionSet.CardSet != null && collectionSet.CardSet.Cards != null && collectionSet.CardSet.Cards.Count > 0)
					{
						cards.AddRange(collectionSet.CardSet.Cards.Select(x => new CardView()
						{
							Id = x.Id,
							SetId = x.SetId,
							BackValue = x.BackValue,
							FrontValue = x.FrontValue,
							ModifiedTime = x.ModifiedTime
						}));
					}
				}
				return new StandardResponse<CardsView>()
				{
					Data = new CardsView()
					{
						CollectionId = collection.Id,
						CollectionName = collection.CollectionName,
						Cards = cards
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
	}
}
