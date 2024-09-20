using FlashCards.Client.Models;
using FlashCards.Shared.Models;
using FlashCards.Shared.Util;
using FlashCards.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace FlashCards.Client.Pages
{
	public partial class CollectionGame : ComponentBase, IDisposable
	{
		[Parameter]
		public string Id { get; set; } = string.Empty;
		private long _id = 0;

		private bool _loading = true;
		private string _errorMessage = string.Empty;
		private bool _prep = true;
		private bool _results = false;
		private int _currentIndex = 0;
		private DateTime _startTime;
		private DateTime _endTime;
		private TimeSpan _currentTime;
		private Timer? _timerInterval;
		private CardViewGame _currentCard = new CardViewGame();
		private CardsView _sourceCards = new CardsView();
		private List<CardViewGame> _cards = [];
		private List<OrderOption> _orderOptions = [];
		public OrderOptionValue SelectOrderOption { get; set; }
		public bool Shuffle { get; set; }
		public bool Timer { get; set; }
		public bool Flipped { get; set; }

		protected override async Task OnInitializedAsync()
		{
			try
			{
				if (!long.TryParse(Id, out var l))
				{
					_errorMessage = "Collection not found";
					return;
				}
				else
				{
					_id = l;
				}

				_orderOptions =
				[
					new OrderOption()
					{
						Name = "Normal",
						Value = OrderOptionValue.Normal
					},
					new OrderOption()
					{
						Name = "Reverse",
						Value = OrderOptionValue.Reverse
					}
				];

				using var getCardsRes = await _httpClient.GetAsync($"/api/Collection/GetCardsForCollection/{_id}");
				var cardsS = await getCardsRes.Content.ReadAsStringAsync();
				if (!getCardsRes.IsSuccessStatusCode || string.IsNullOrWhiteSpace(cardsS) || !cardsS.StartsWith('{'))
				{
					_errorMessage = $"Failed to load cards. ({getCardsRes.StatusCode})";
					return;
				}
				var cards = JsonSerializer.Deserialize<StandardResponse<CardsView>>(cardsS, DefaultJsonOptions.DefaultOptions);
				if (cards?.Data?.Cards == null || !cards.Success)
				{
					_errorMessage = string.IsNullOrWhiteSpace(cards?.Message) ? $"Failed to load cards. ({getCardsRes.StatusCode})" : cards.Message;
					return;
				}
				_sourceCards = cards.Data;
			}
			finally
			{
				_loading = false;
				StateHasChanged();
			}
		}

		private void OnStartClicked()
		{
			_cards = _sourceCards.Cards.Select(x => new CardViewGame(x)).ToList();
			if (SelectOrderOption == OrderOptionValue.Reverse)
				_cards.Reverse();
			if (Shuffle)
				_cards = [.. _cards.OrderBy(x => Random.Shared.Next()).OrderByDescending(x => Random.Shared.Next())];
			if (Flipped)
				_cards.ForEach(x => x.Flipped = true);
			_startTime = DateTime.UtcNow;
			_currentIndex = 0;
			_currentCard = _cards[0];
			_currentCard.StartTime = DateTime.UtcNow;
			_prep = false;
			_timerInterval = new Timer
			{
				Interval = 245,
				Enabled = false,
				AutoReset = false
			};
			_timerInterval.Start();
			_timerInterval.Elapsed += OnTimerInterval;
			StateHasChanged();
		}

		private void CardClicked(long id)
		{
			var card = _cards.FirstOrDefault(x => x.Id == id);
			if (card != null)
				card.Flipped = !card.Flipped;
			StateHasChanged();
		}

		private void OnNextCardClicked(bool correct)
		{
			_currentIndex++;
			_currentCard.Correct = correct;
			_currentCard.EndTime = DateTime.UtcNow;
			if (_currentIndex < _cards.Count)
			{
				_currentCard = _cards[_currentIndex];
			}
			else
			{
				_endTime = DateTime.UtcNow;
				_results = true;
			}
			StateHasChanged();
		}

		private void OnSaveClicked()
		{
			//TODO: Save
			ResetToPrep();
		}

		private void OnCancelClicked()
		{
			ResetToPrep();
		}

		private void ResetToPrep()
		{
			if (_timerInterval != null)
			{
				_timerInterval.Stop();
				_timerInterval.Elapsed -= OnTimerInterval;
				_timerInterval.Dispose();
				_timerInterval = null;
			}
			_results = false;
			_prep = true;
			StateHasChanged();
		}

		private async void OnTimerInterval(object? sender, ElapsedEventArgs e)
		{
			await InvokeAsync(() =>
			{
				_currentTime = DateTime.UtcNow - _startTime;
				StateHasChanged();
			});
			_timerInterval?.Start();
		}

		public void Dispose()
		{
			try
			{
				if (_timerInterval != null)
				{
					_timerInterval.Stop();
					_timerInterval.Elapsed -= OnTimerInterval;
					_timerInterval.Dispose();
				}
			}
			catch { }
			GC.SuppressFinalize(this);
		}

		private class OrderOption
		{
			public OrderOptionValue Value { get; set; }
			public string Name { get; set; } = string.Empty;
		}

		public enum OrderOptionValue
		{
			Normal,
			Reverse
		}
	}
}
