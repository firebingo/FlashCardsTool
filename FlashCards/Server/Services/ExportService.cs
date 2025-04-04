using FlashCards.Server.Data;
using FlashCards.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashCards.Server.Services
{
	public class ExportService
	{
		readonly ILogger<ExportService> _logger;
		readonly ServiceDbContext _dbContext;

		public ExportService(ILogger<ExportService> logger, ServiceDbContext dbContext)
		{
			_logger = logger;
			_dbContext = dbContext;
		}

		public async Task<StandardResponse<MemoryStream>> ExportAnkiPlainText(ExportAnkiRequest request)
		{
			var standardMessage = $"ExportService:ExportAnkiPlainText({JsonSerializer.Serialize(request)})";
			try
			{
				var sets = await _dbContext.CardSet.Include(x => x.Cards).Where(x => x.UserId == request.UserId && EF.Constant(request.SetIds).Contains(x.Id)).ToListAsync();
				if (sets.Count == 0)
				{
					_logger.LogWarning($"No decks to export {standardMessage}");
					return new StandardResponse<MemoryStream>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.BadRequest,
						Message = "NO_DECKS_TO_EXPORT"
					};
				}

				var cards = sets.Select(x => x.Cards?.ToList() ?? []).SelectMany(x => x);
				if (cards.Count() == 0)
				{
					_logger.LogWarning($"No cards to export {standardMessage}");
					return new StandardResponse<MemoryStream>()
					{
						Success = false,
						StatusCode = System.Net.HttpStatusCode.BadRequest,
						Message = "NO_CARDS_TO_EXPORT"
					};
				}

				var stream = new MemoryStream();
				TextWriter tw = new StreamWriter(stream);

				tw.WriteLine("#separator:tab");
				tw.WriteLine("#html:true");

				var usedTimestamps = new List<long>();
				foreach (var card in cards.OrderBy(x => x.CreatedTime))
				{
					var front = card.FrontValue ?? string.Empty;
					var back = card.BackValue ?? string.Empty;
					tw.WriteLine($"{front.Replace("\t", "&nbsp;").Replace("\n", "<br>")}	{back.Replace("\t", "&nbsp;").Replace("\n", "<br>")}");
				}
				tw.Flush();
				stream.Seek(0, SeekOrigin.Begin);

				return new StandardResponse<MemoryStream>()
				{
					Data = stream
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception: {standardMessage}");
				return new StandardResponse<MemoryStream>()
				{
					Success = false,
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Message = "EXCEPTION"
				};
			}
		}
	}
}
