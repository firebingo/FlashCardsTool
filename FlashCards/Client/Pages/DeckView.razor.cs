using FlashCards.Client.Pages.Modals;
using FlashCards.Shared.Models;
using FlashCards.Shared.Util;
using FlashCards.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashCards.Client.Pages
{
	public partial class DeckView : ComponentBase
	{
		private bool _loading;
		private string _errorMessage = string.Empty;
		private CardsView _cards = new CardsView();

		[Parameter]
		public string Id { get; set; } = string.Empty;
		private long _id = 0;

		protected override void OnInitialized()
		{
			if (!long.TryParse(Id, out var l))
			{
				_errorMessage = "Deck not found";
			}
			else
			{
				_id = l;
				_loading = true;
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await LoadCards();
			}
		}

		private async Task LoadCards()
		{
			_loading = true;
			_errorMessage = string.Empty;
			try
			{
				await LoadCardsBackground();
			}
			catch
			{
				_errorMessage = $"Failed to load cards.";
			}
			finally
			{
				_loading = false;
				StateHasChanged();
			}
		}

		private async Task LoadCardsBackground(bool background = false)
		{
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
			_cards = cards.Data;
			if (background)
				await InvokeAsync(StateHasChanged);
		}

		private void CardClicked(long id)
		{
			var card = _cards.Cards.FirstOrDefault(x => x.Id == id);
			if (card != null)
			{
				card.Flipped = !card.Flipped;
				card.DeleteConfirm = false;
			}
			StateHasChanged();
		}

		private async Task NewCardClicked()
		{
			await _dialogService.OpenAsync<NewCard>("NewCard",
				new Dictionary<string, object>()
				{
					{ "SetId", _id },
					{ "CompleteCallback", (CardView card) => OnNewCardComplete(card) }
				},
				new DialogOptions()
				{
					ShowTitle = false,
					ShowClose = false,
					CloseDialogOnOverlayClick = true
				});
		}

		private Task OnNewCardComplete(CardView? card)
		{
			if (card == null)
				return Task.CompletedTask;

			_cards.Cards.Add(card);
			StateHasChanged();
			_ = Task.Run(async () => await LoadCardsBackground(true));
			return Task.CompletedTask;
		}

		private async Task EditClicked(long id)
		{
			var card = _cards.Cards.FirstOrDefault(x => x.Id == id);
			if (card == null)
				return;

			await _dialogService.OpenAsync<EditCard>("EditCard",
				new Dictionary<string, object>()
				{
					{ "SetId", _id },
					{ "Card", card },
					{ "CompleteCallback", (CardView card) => OnEditComplete(card) }
				},
				new DialogOptions()
				{
					ShowTitle = false,
					ShowClose = false,
					CloseDialogOnOverlayClick = true
				});
		}

		private Task OnEditComplete(CardView? card)
		{
			if (card == null)
				return Task.CompletedTask;

			var oldCard = _cards.Cards.FirstOrDefault(x => x.Id == card.Id);

			if (oldCard == null)
				return Task.CompletedTask;

			oldCard.DeleteConfirm = false;
			oldCard.FrontValue = card.FrontValue;
			oldCard.BackValue = card.BackValue;
			oldCard.ModifiedTime = card.ModifiedTime;
			StateHasChanged();
			_ = Task.Run(async () => await LoadCardsBackground(true));
			return Task.CompletedTask;
		}

		private void DeleteClicked(long id)
		{
			var card = _cards.Cards.FirstOrDefault(x => x.Id == id);
			if (card == null)
				return;

			card.DeleteConfirm = true;
			StateHasChanged();
		}

		private async Task ConfirmDeleteClicked(long id)
		{
			var card = _cards.Cards.FirstOrDefault(x => x.Id == id);
			if (card == null)
				return;

			card.DeleteConfirm = false;
			card.Loading = true;
			var postModel = new DeleteCardsRequest()
			{
				SetId = _id,
				Cards =
				[
					new DeleteCardsCard() { Id = id }
				]
			};
			using var content = new StringContent(JsonSerializer.Serialize(postModel, DefaultJsonOptions.DefaultOptions), Encoding.UTF8, "application/json");
			using var request = new HttpRequestMessage
			{
				Method = HttpMethod.Delete,
				RequestUri = new Uri(_httpClient.BaseAddress!, "/api/card/DeleteCards"),
				Content = content
			};
			using var res = await _httpClient.SendAsync(request);
			var resS = await res.Content.ReadAsStringAsync();
			if (!res.IsSuccessStatusCode || string.IsNullOrWhiteSpace(resS) || !resS.StartsWith('{'))
			{
				_errorMessage = $"Error deleting card. ({res.StatusCode})";
				_loading = false;
				return;
			}
			var result = JsonSerializer.Deserialize<StandardResponse>(resS, DefaultJsonOptions.DefaultOptions);
			if (result?.Success != true)
			{
				_errorMessage = $"Error deleting card. ({res.StatusCode})";
				_loading = false;
				return;
			}

			_cards.Cards = _cards.Cards.Where(x => x.Id != card.Id).ToList();
			StateHasChanged();
			_ = Task.Run(async () => await LoadCardsBackground(true));
		}

		private void OnPlayClicked()
		{
			_navManager.NavigateTo($"/Decks/{_id}/Game");
		}
	}
}
