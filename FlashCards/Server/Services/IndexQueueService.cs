using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FlashCards.Server.Services
{
	public class IndexQueueService
	{
		private readonly IHostApplicationLifetime _appLifetime;
		private readonly ConcurrentQueue<KeyValuePair<long, long>> _indexQueue;
		private readonly ConcurrentDictionary<long, object?> _queuedIds;

		public IndexQueueService(IHostApplicationLifetime appLifetime)
		{
			_appLifetime = appLifetime;
			_indexQueue = new ConcurrentQueue<KeyValuePair<long, long>>();
			_queuedIds = new ConcurrentDictionary<long, object?>();
		}

		public int QueueIndex(long user)
		{
			if (_appLifetime.ApplicationStopping.IsCancellationRequested || _appLifetime.ApplicationStopped.IsCancellationRequested)
				return 1;
			if (_queuedIds.ContainsKey(user))
				return 2;
			_queuedIds.TryAdd(user, null);
			_indexQueue.Enqueue(new KeyValuePair<long, long>(user, user));
			return 0;
		}

		public Dictionary<long, long> GetToIndexFromQueue(int count)
		{
			var retval = new Dictionary<long, long>();
			for (var i = 0; i < count; ++i)
			{
				if (_appLifetime.ApplicationStopping.IsCancellationRequested || _appLifetime.ApplicationStopped.IsCancellationRequested)
					break;

				if (_indexQueue.TryDequeue(out var o))
				{
					_queuedIds.TryRemove(o.Key, out _);
					retval.Add(o.Key, o.Value);
				}
			}
			return retval;
		}

		public Dictionary<long, long> EmptyQueue()
		{
			var retval = new Dictionary<long, long>();

			while (_indexQueue.TryDequeue(out var o))
			{
				_queuedIds.TryRemove(o.Key, out _);
				retval.Add(o.Key, o.Value);
			};
			return retval;
		}
	}
}
