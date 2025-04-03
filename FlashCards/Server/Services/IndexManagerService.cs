using FlashCards.Server.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlashCards.Server.Services
{
	public class IndexManagerService : IHostedService, IDisposable
	{
		private readonly ILogger _logger;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IHostApplicationLifetime _appLifetime;
		private readonly IndexQueueService _indexQueueService;
		private CancellationTokenSource? _cancelSource;
		private CancellationToken _cancelToken;
		private readonly int _maxConcurrentCount = 5;
		private bool _running;
		private Timer? _timer;
		private bool _isDisposed;

		public IndexManagerService(ILogger<IndexManagerService> logger,
			IOptions<AppSettings> appSettings,
			IServiceScopeFactory scopeFactory,
			IHostApplicationLifetime appLifetime,
			IndexQueueService indexQueueService)
		{
			_logger = logger;
			_scopeFactory = scopeFactory;
			_appLifetime = appLifetime;
			_indexQueueService = indexQueueService;
		}

		public Task StartAsync(CancellationToken processCancelToken)
		{
			_cancelSource = new CancellationTokenSource();
			_cancelToken = _cancelSource.Token;
			_timer = new Timer(async (state) =>
			{
				_running = true;
				await DoWork();
				_running = false;
			}, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

			return Task.CompletedTask;
		}

		private async Task DoWork()
		{
			if (_cancelToken.IsCancellationRequested)
			{
				return;
			}

			try
			{
				var toIndex = _indexQueueService.GetToIndexFromQueue(_maxConcurrentCount);
				await Parallel.ForEachAsync(toIndex, _cancelToken, async (user, token) =>
				{
					if (token.IsCancellationRequested || _cancelToken.IsCancellationRequested)
						return;
					using var scope = _scopeFactory.CreateScope();
					var service = scope.ServiceProvider.GetRequiredService<IndexService>();
					await service.IndexUser(user.Value, _cancelToken);
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Exception in IndexManagerService.DoWork");
			}
		}

		public async Task StopAsync(CancellationToken processCancelToken)
		{
			_timer?.Change(Timeout.Infinite, 0);
			_cancelSource?.Cancel();
			_ = _indexQueueService.EmptyQueue();

			using var scope = _scopeFactory.CreateScope();
			//Give the process a little bit of time to cancel itself
			var maxWaitTime = 10000;
			var waitTime = 0;
			while (_running && waitTime < maxWaitTime && !processCancelToken.IsCancellationRequested)
			{
				waitTime += 500;
				await Task.Delay(500, processCancelToken);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed) return;

			if (disposing)
			{

			}

			_isDisposed = true;
		}

		~IndexManagerService()
		{
			Dispose(false);
		}
	}
}
