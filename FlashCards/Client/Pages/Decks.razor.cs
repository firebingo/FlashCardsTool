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
	public partial class Decks : ComponentBase
	{
		private bool _loading;
		private string _errorMessage = string.Empty;
		private List<CardSetView> _decks = [];

		protected override void OnInitialized()
		{
			_loading = true;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await LoadDecks();
			}
		}

		private async Task LoadDecks()
		{
			_loading = true;
			_errorMessage = string.Empty;
			try
			{
				using var getCardsRes = await _httpClient.GetAsync("/api/Card/GetCardSets");
				var cardsS = await getCardsRes.Content.ReadAsStringAsync();
				if (!getCardsRes.IsSuccessStatusCode || string.IsNullOrWhiteSpace(cardsS) || !cardsS.StartsWith('{'))
				{
					_errorMessage = $"Failed to load decks. ({getCardsRes.StatusCode})";
					return;
				}
				var decks = JsonSerializer.Deserialize<StandardResponse<List<CardSetView>>>(cardsS, DefaultJsonOptions.DefaultOptions);
				if (decks?.Data == null || !decks.Success)
				{
					_errorMessage = string.IsNullOrWhiteSpace(decks?.Message) ? $"Failed to load decks. ({getCardsRes.StatusCode})" : decks.Message;
					return;
				}
				_decks = decks.Data;
			}
			catch
			{
				_errorMessage = $"Failed to load decks.";
			}
			finally
			{
				_loading = false;
				StateHasChanged();
			}
		}

		private async Task OnNewDeckClicked()
		{
			await _dialogService.OpenAsync<NewDeck>("NewDeck",
				new Dictionary<string, object>()
				{
					{ "CompleteCallback", () => OnNewDeckComplete() }
				},
				new DialogOptions()
				{
					ShowTitle = false,
					ShowClose = false,
					CloseDialogOnOverlayClick = true
				});
		}

		private async Task OnNewDeckComplete()
		{
			await LoadDecks();
		}

		private void OnDeckClicked(long id)
		{
			var deck = _decks.FirstOrDefault(x => x.Id == id);
			if (deck != null)
				_navManager.NavigateTo($"/Decks/{id}");
		}

		private async Task OnDeckDeleteClicked(long id)
		{
			var deck = _decks.FirstOrDefault(x => x.Id == id);
			if (deck == null)
				return;

			await _dialogService.OpenAsync<ConfirmDeleteDeck>("ConfirmDeleteDeck",
				new Dictionary<string, object>()
				{
					{ "Deck", deck },
					{ "CompleteCallback", () => OnNewDeckComplete() }
				},
				new DialogOptions()
				{
					ShowTitle = false,
					ShowClose = false,
					CloseDialogOnOverlayClick = true
				});
		}
	}
}
