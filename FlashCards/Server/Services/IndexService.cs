using FlashCards.Server.Data;
using FlashCards.Server.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Ja;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCards.Server.Services
{
	public class IndexService
	{
		public readonly static LuceneVersion LuceneVersion = LuceneVersion.LUCENE_48;

		readonly ILogger<IndexService> _logger;
		readonly ServiceDbContext _dbContext;
		readonly string _folderLocation = string.Empty;

		public IndexService(ILogger<IndexService> logger, ServiceDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
			if (!string.IsNullOrWhiteSpace(AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString()))
				_folderLocation = $"{AppDomain.CurrentDomain.GetData("DataDirectory")}\\Index";
		}

		private static IndexWriter GetWriter(FSDirectory dir)
		{
			var enAnalyzer = new StandardAnalyzer(LuceneVersion);
			var analyzerPerField = new Dictionary<string, Analyzer>
			{
				["text_jp"] = new JapaneseAnalyzer(LuceneVersion),
				["text2_jp"] = new JapaneseAnalyzer(LuceneVersion)
			};
			var aWrapper = new PerFieldAnalyzerWrapper(enAnalyzer, analyzerPerField);
			var indexConfig = new IndexWriterConfig(LuceneVersion, aWrapper)
			{
				OpenMode = OpenMode.CREATE
			};
			return new IndexWriter(dir, indexConfig);
		}

		public async Task IndexUser(long userId, CancellationToken cancelToken)
		{
			if (cancelToken.IsCancellationRequested)
				return;

			if (string.IsNullOrWhiteSpace(_folderLocation))
			{
				_logger.LogError($"Missing index directory");
				return;
			}

			try
			{
				var user = (await _dbContext.Users.Where(x => x.Id == userId).ToListAsync(cancelToken)).FirstOrDefault();
				if (user == null || user.Disabled)
				{
					_logger.LogError($"Can't index user ({userId})");
					return;
				}

				var userIndexLocation = Path.Combine(_folderLocation, $"{userId}");
				System.IO.Directory.CreateDirectory(userIndexLocation);

				if (cancelToken.IsCancellationRequested)
					return;

				using var dir = FSDirectory.Open(userIndexLocation);
				using var writer = GetWriter(dir);

				var decks = await _dbContext.CardSet.Include(x => x.Cards).Where(x => x.UserId == userId).ToListAsync(cancelToken);
				var collections = (await _dbContext.CardCollection
						.Include(x => x.CollectionSets)
						!.ThenInclude(x => x.CardSet)
						!.ThenInclude(x => x!.Cards)
						.Where(x => x.UserId == userId)
						.ToListAsync(cancelToken));

				if (cancelToken.IsCancellationRequested)
					return;

				if (decks.Count > 0)
				{
					Parallel.ForEach(decks, (deck) =>
					{
						var d = new Document()
						{
							new StringField("text", deck.SetName, Field.Store.NO),
							new StringField("text_jp", deck.SetName, Field.Store.NO),
							new Int64Field("setid", deck.Id, Field.Store.YES)
						};
						writer.AddDocument(d);
						if (deck.Cards?.Count > 0)
						{
							foreach (var card in deck.Cards)
							{
								if (!string.IsNullOrWhiteSpace(card.FrontValue) || !string.IsNullOrWhiteSpace(card.BackValue))
								{
									var cd = new Document()
									{
										new TextField("text", card.FrontValue ?? string.Empty, Field.Store.NO),
										new TextField("text2", card.BackValue ?? string.Empty, Field.Store.NO),
										new StringField("text_jp", card.FrontValue ?? string.Empty, Field.Store.NO),
										new StringField("text2_jp", card.BackValue ?? string.Empty, Field.Store.NO),
										new Int64Field("cardid", card.Id, Field.Store.YES),
										new Int64Field("setid", deck.Id, Field.Store.YES)
									};
									writer.AddDocument(cd);
								}
							}
						}
					});
				}

				if (collections.Count > 0)
				{
					Parallel.ForEach(collections, (collection) =>
					{
						var d = new Document()
						{
							new StringField("text", collection.CollectionName, Field.Store.NO),
							new StringField("text_jp", collection.CollectionName, Field.Store.NO),
							new Int64Field("collectionid", collection.Id, Field.Store.YES)
						};
						writer.AddDocument(d);
						if (collection.CollectionSets?.Count > 0)
						{
							foreach (var deck in collection.CollectionSets.Where(x => x.CardSet != null))
							{
								var dd = new Document()
								{
									new StringField("text", deck.CardSet!.SetName, Field.Store.NO),
									new StringField("text_jp", deck.CardSet!.SetName, Field.Store.NO),
									new Int64Field("setid", deck.SetId, Field.Store.YES),
									new Int64Field("collectionid", collection.Id, Field.Store.YES)
								};
								writer.AddDocument(dd);
								if (deck.CardSet.Cards?.Count > 0)
								{
									foreach (var card in deck.CardSet.Cards)
									{
										var cd = new Document()
										{
											new TextField("text", card.FrontValue, Field.Store.NO),
											new TextField("text2", card.BackValue, Field.Store.NO),
											new StringField("text_jp", card.FrontValue, Field.Store.NO),
											new StringField("text2_jp", card.BackValue, Field.Store.NO),
											new Int64Field("cardid", card.Id, Field.Store.YES),
											new Int64Field("setid", deck.SetId, Field.Store.YES),
											new Int64Field("collectionid", collection.Id, Field.Store.YES)
										};
										writer.AddDocument(cd);
									}
								}
							}
						}
					});
				}

				writer.Commit();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception indexing user ({userId})");
			}
		}

		public async Task<List<IndexResult>> SearchIndex(string query, long userId)
		{
			try
			{
				List<IndexResult> results = [];
				var userIndexLocation = Path.Combine(_folderLocation, $"{userId}");
				if (!System.IO.Directory.Exists(userIndexLocation))
				{
					_logger.LogWarning($"User searching with no index built ({userId}) query:\n{query}");
					return results;
				}
				using var dir = FSDirectory.Open(userIndexLocation);
				using var writer = GetWriter(dir);

				var phrase = new MultiPhraseQuery
				{
					new Term("text", query),
					new Term("text_jp", query),
					new Term("text2", query),
					new Term("text2_jp", query)
				};

				using var rdr = writer.GetReader(true);
				var searcher = new IndexSearcher(rdr);
				var hits = searcher.Search(phrase, 50).ScoreDocs;
				Document? foundDoc = null;
				foreach (var hit in hits)
				{
					foundDoc = searcher.Doc(hit.Doc);
					var cardIdField = foundDoc.Fields.FirstOrDefault(x => x.Name == "cardid");
					var setIdField = foundDoc.Fields.FirstOrDefault(x => x.Name == "setid");
					var collectionIdField = foundDoc.Fields.FirstOrDefault(x => x.Name == "collectionid");
					if (cardIdField != null)
					{
						var cardId = cardIdField.GetInt64Value();
						var setId = setIdField!.GetInt64Value();
						var collectionId = collectionIdField?.GetInt64Value();
						var card = (await _dbContext.Card.Where(x => x.Id == cardId && x.SetId == setId).ToListAsync()).FirstOrDefault();
						if (card != null)
						{
							results.Add(new IndexResult()
							{
								Text = card.FrontValue ?? string.Empty,
								Text2 = card.BackValue ?? string.Empty,
								IsCard = true,
								CardId = cardId,
								SetId = setId,
								CollectionId = collectionId,
							});
						}
					}
					else if (cardIdField == null && setIdField != null && collectionIdField == null)
					{
						var setId = setIdField!.GetInt64Value();
						var set = (await _dbContext.CardSet.Where(x => x.Id == setId).ToListAsync()).FirstOrDefault();
						if (set != null)
						{
							results.Add(new IndexResult()
							{
								Text = set.SetName,
								IsSet = true,
								SetId = setId
							});
						}
					}
					else if (cardIdField == null && setIdField == null && collectionIdField != null)
					{
						var collectionId = collectionIdField.GetInt64Value();
						var collection = (await _dbContext.CardCollection.Where(x => x.Id == collectionId).ToListAsync()).FirstOrDefault();
						if (collection != null)
						{
							results.Add(new IndexResult()
							{
								Text = collection.CollectionName,
								IsCollection = true,
								CollectionId = collectionId
							});
						}
					}
				}

				return results;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception searching for user ({userId}) query:\n{query}");
				return [];
			}
		}
	}
}
