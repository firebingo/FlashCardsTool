using FlashCards.Client.Models;
using FlashCards.Client.Util;
using FlashCards.Shared.Models;
using FlashCards.Shared.Util;
using FlashCards.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace FlashCards.Client.Pages
{
	public partial class DeckGame : ComponentBase, IDisposable
	{
		[Parameter]
		public string Id { get; set; } = string.Empty;
		private long _id = 0;

		private UserSettingsView _userSettings = new UserSettingsView();

		private bool _loading = true;
		private string _errorMessage = string.Empty;
		private string _optionsMessage = string.Empty;
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
		private int _filteredCount = 0;
		private List<OrderOption> _orderOptions = [];
		public OrderOptionValue SelectOrderOption { get; set; }
		public bool Shuffle { get; set; }
		public bool Timer { get; set; }
		public bool ShowProgress { get; set; }
		public bool Flipped { get; set; }
		public int PercentThreshold { get; set; } = 100;
		public bool IncludeAllBelowThreshold { get; set; } = true;
		public bool RepeatMissed { get; set; } = false;

		protected override async Task OnInitializedAsync()
		{
			try
			{
				if (!long.TryParse(Id, out var l))
				{
					_errorMessage = "Deck not found";
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

				using var getCardsRes = await _httpClient.GetAsync($"/api/Card/GetCardsForSet/{_id}");
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
				_filteredCount = _sourceCards.Cards.Count;
			}
			finally
			{
				_loading = false;
				StateHasChanged();
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				_userSettings = await _userSettingsService.GetUserSettings();
				StateHasChanged();
			}
		}

		private void OnAfterFilterChanged()
		{
			if (RepeatMissed)
			{
				_filteredCount = _cards.Where(x => !x.Correct).Count();
			}
			else
			{
				if (IncludeAllBelowThreshold)
				{
					_filteredCount = _sourceCards.Cards
					.Where(x => x.PassPercent <= (PercentThreshold / 100.0f) ||
					(x.PassCount + x.MissCount) < _userSettings.ColorCardThreshold)
					.Count();
				}
				else
				{
					_filteredCount = _sourceCards.Cards
					.Where(x => x.PassPercent <= (PercentThreshold / 100.0f) &&
					(x.PassCount + x.MissCount) >= _userSettings.ColorCardThreshold)
					.Count();
				}
			}
			StateHasChanged();
		}

		private void OnStartClicked()
		{
			_optionsMessage = string.Empty;
			StateHasChanged();
			if (RepeatMissed)
			{
				_cards = _cards.Where(x => !x.Correct)
					.Select(x => new CardViewGame(_sourceCards.Cards.First(s => s.Id == x.Id && s.SetId == x.SetId)))
					.ToList();
			}
			else
			{
				if (IncludeAllBelowThreshold)
				{
					_cards = _sourceCards.Cards.Select(x => new CardViewGame(x))
					.Where(x => x.PassPercent <= (PercentThreshold / 100.0f) ||
					(IncludeAllBelowThreshold && (x.PassCount + x.MissCount) < _userSettings.ColorCardThreshold))
					.ToList();
				}
				else
				{
					_cards = _sourceCards.Cards.Select(x => new CardViewGame(x))
					.Where(x => x.PassPercent <= (PercentThreshold / 100.0f) &&
					(x.PassCount + x.MissCount) >= _userSettings.ColorCardThreshold)
					.ToList();
				}
			}

			if (_cards.Count == 0)
			{
				_optionsMessage = "No valid cards for filter.";
				StateHasChanged();
				return;
			}
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

		private async Task OnSaveClicked()
		{
			_loading = true;
			try
			{
				foreach (var card in _cards)
				{
					var c = _sourceCards.Cards.FirstOrDefault(x => x.Id == card.Id && x.SetId == card.SetId);
					if (c != null)
					{
						if (card.Correct)
						{
							c.PassCount++;
							c.LastPass = DateTime.UtcNow;
						}
						else
						{
							c.MissCount++;
							c.LastMiss = DateTime.UtcNow;
						}
					}
				}
				var putModel = new RecordPlayRequest()
				{
					SetId = _id,
					PlayTime = _endTime - _startTime,
					Cards = _cards.Select(x => new RecordPlayRequestCard()
					{
						CardId = x.Id,
						SetId = x.SetId,
						Pass = x.Correct
					}).ToList()
				};

				using var content = new StringContent(JsonSerializer.Serialize(putModel, DefaultJsonOptions.DefaultOptions), Encoding.UTF8, "application/json");
				using var putPlayRes = await _httpClient.PutAsync("/api/playstats/recordplay", content);
				var playS = await putPlayRes.Content.ReadAsStringAsync();
				if (!putPlayRes.IsSuccessStatusCode || string.IsNullOrWhiteSpace(playS) || !playS.StartsWith('{'))
				{
					_errorMessage = $"Failed to save result. ({putPlayRes.StatusCode})";
					return;
				}
			}
			catch
			{
				_errorMessage = $"Failed to save result. (EX)";
			}
			finally
			{
				_loading = false;
				StateHasChanged();
			}
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
			if (RepeatMissed && !_cards.Any(x => !x.Correct))
			{
				RepeatMissed = false;
			}
			OnAfterFilterChanged();
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

		public string GetCardStyle(CardViewGame card)
		{
			if (_userSettings.ShowCardColorInGame)
				return ClientUtil.GetCardStyle(card, _userSettings);
			return string.Empty;
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
