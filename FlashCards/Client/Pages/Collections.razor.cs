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
	public partial class Collections : ComponentBase
	{
		private bool _loading;
		private string _errorMessage = string.Empty;
		private List<CardCollectionView> _collections = [];

		protected override void OnInitialized()
		{
			_loading = true;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await LoadCollections();
			}
		}

		private async Task LoadCollections(bool background = false)
		{
			_loading = true;
			_errorMessage = string.Empty;
			try
			{
				using var getCardsRes = await _httpClient.GetAsync("/api/collection/GetCardCollections");
				var cardsS = await getCardsRes.Content.ReadAsStringAsync();
				if (!getCardsRes.IsSuccessStatusCode || string.IsNullOrWhiteSpace(cardsS) || !cardsS.StartsWith('{'))
				{
					_errorMessage = $"Failed to load collections. ({getCardsRes.StatusCode})";
					return;
				}
				var collections = JsonSerializer.Deserialize<StandardResponse<List<CardCollectionView>>>(cardsS, DefaultJsonOptions.DefaultOptions);
				if (collections?.Data == null || !collections.Success)
				{
					_errorMessage = string.IsNullOrWhiteSpace(collections?.Message) ? $"Failed to load collections. ({getCardsRes.StatusCode})" : collections.Message;
					return;
				}
				_collections = collections.Data;
			}
			catch
			{
				_errorMessage = $"Failed to load collections.";
			}
			finally
			{
				_loading = false;
				if (background)
					await InvokeAsync(StateHasChanged);
				else
					StateHasChanged();
			}
		}

		private async Task OnNewCollectionClicked()
		{
			await _dialogService.OpenAsync<NewCollection>("NewCollection",
				new Dictionary<string, object>()
				{
					{ "CompleteCallback", () => OnNewCollectionComplete() }
				},
				new DialogOptions()
				{
					ShowTitle = false,
					ShowClose = false,
					CloseDialogOnOverlayClick = true
				});
		}

		private async Task OnNewCollectionComplete()
		{
			await LoadCollections();
		}

		private Task OnCollectionEditComplete(long id, string name)
		{
			var oldCollection = _collections.FirstOrDefault(x => x.Id == id);
			if (oldCollection == null)
				return Task.CompletedTask;

			oldCollection.CollectionName = name;
			StateHasChanged();

			_ = Task.Run(() => LoadCollections(true));
			return Task.CompletedTask;
		}

		private async Task EditClicked(long id)
		{
			var collection = _collections.FirstOrDefault(x => x.Id == id);
			if (collection == null)
				return;

			await _dialogService.OpenAsync<EditDeck>("EditCard",
				new Dictionary<string, object>()
				{
					{ "SetId", id },
					{ "Name", collection.CollectionName },
					{ "CompleteCallback", (string name) => OnCollectionEditComplete(id, name) }
				},
				new DialogOptions()
				{
					ShowTitle = false,
					ShowClose = false,
					CloseDialogOnOverlayClick = true
				});
		}

		private void OnCollectionClicked(long id)
		{
			var collection = _collections.FirstOrDefault(x => x.Id == id);
			if (collection != null)
				_navManager.NavigateTo($"/Collections/{id}");
		}

		private async Task OnCollectionDeleteClicked(long id)
		{
			var collection = _collections.FirstOrDefault(x => x.Id == id);
			if (collection == null)
				return;

			await _dialogService.OpenAsync<ConfirmDeleteCollection>("ConfirmDeleteCollection",
				new Dictionary<string, object>()
				{
					{ "Collection", collection },
					{ "CompleteCallback", () => OnNewCollectionComplete() }
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
