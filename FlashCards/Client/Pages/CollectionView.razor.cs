using FlashCards.Client.Pages.Modals;
using FlashCards.Shared.Models;
using FlashCards.Shared.Util;
using FlashCards.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashCards.Client.Pages
{
	public partial class CollectionView : ComponentBase
	{
		private bool _loading;
		private string _errorMessage = string.Empty;
		private CardCollectionView _collection = new CardCollectionView();
		private CardsView _cards = new CardsView();

		[Parameter]
		public string Id { get; set; } = string.Empty;
		private long _id = 0;

		protected override void OnInitialized()
		{
			if (!long.TryParse(Id, out var l))
			{
				_errorMessage = "Collection not found";
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
				var res = await LoadCollectionView();
				if (!res)
					return;
				await LoadCards();
			}
		}

		private async Task<bool> LoadCollectionView()
		{
			_loading = true;
			_errorMessage = string.Empty;
			try
			{
				using var getCollectionRes = await _httpClient.GetAsync($"/api/Collection/GetCardCollection/{_id}");
				var collectionS = await getCollectionRes.Content.ReadAsStringAsync();
				if (!getCollectionRes.IsSuccessStatusCode || string.IsNullOrWhiteSpace(collectionS) || !collectionS.StartsWith('{'))
				{
					_errorMessage = $"Failed to load collection. ({getCollectionRes.StatusCode})";
					return false;
				}
				var collectionView = JsonSerializer.Deserialize<StandardResponse<CardCollectionView>>(collectionS, DefaultJsonOptions.DefaultOptions);
				if (collectionView?.Data == null || !collectionView.Success)
				{
					_errorMessage = string.IsNullOrWhiteSpace(collectionView?.Message) ? $"Failed to load collection. ({getCollectionRes.StatusCode})" : collectionView.Message;
					return false;
				}

				_collection = collectionView.Data;

				return true;
			}
			catch
			{
				_errorMessage = $"Failed to load collection.";
				return false;
			}
			finally
			{
				_loading = false;
				StateHasChanged();
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

		private async Task EditCardClicked(long id)
		{
			var card = _cards.Cards.FirstOrDefault(x => x.Id == id);
			if (card == null)
				return;

			await _dialogService.OpenAsync<EditCard>("EditCard",
				new Dictionary<string, object>()
				{
					{ "SetId", card.SetId },
					{ "Card", card },
					{ "CompleteCallback", (CardView card) => OnEditCardComplete(card) }
				},
				new DialogOptions()
				{
					ShowTitle = false,
					ShowClose = false,
					CloseDialogOnOverlayClick = true
				});
		}

		private Task OnEditCardComplete(CardView? card)
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

		private async Task OnEditClicked()
		{
			await _dialogService.OpenAsync<EditCollection>("EditCollection",
				new Dictionary<string, object>()
				{
					{ "Collection", _collection },
					{ "CompleteCallback", (string name) => OnEditComplete(name) }
				},
				new DialogOptions()
				{
					ShowTitle = false,
					ShowClose = false,
					CloseDialogOnOverlayClick = true
				});
		}

		private async Task OnEditComplete(string name)
		{
			if (!string.IsNullOrWhiteSpace(name))
				_collection.CollectionName = name;

			await LoadCollectionView();
			await LoadCards();
		}

		private void OnPlayClicked()
		{
			_navManager.NavigateTo($"/Collections/{_id}/Game");
		}
	}
}
