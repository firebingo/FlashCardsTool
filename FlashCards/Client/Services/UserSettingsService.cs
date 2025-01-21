using FlashCards.Client.Models;
using FlashCards.Shared.Models;
using FlashCards.Shared.Models.Auth;
using FlashCards.Shared.Util;
using FlashCards.Shared.ViewModels;
using Microsoft.JSInterop;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashCards.Client.Services
{
	public class UserSettingsService
	{
		private readonly HttpClient _httpClient;
		private readonly IJSRuntime _jsRuntime;

		public UserSettingsService(HttpClient httpClient, IJSRuntime jsRuntime)
		{
			_httpClient = httpClient;
			_jsRuntime = jsRuntime;
		}

		public async Task<UserSettingsView> GetUserSettings()
		{
			var retVal = new UserSettingsView();
			var cacheSettings = await _jsRuntime.InvokeAsync<UserSettingsView>("getLocalStorageCache", "usersettings");
			if (cacheSettings == null)
			{
				using var getSetRes = await _httpClient.GetAsync("/api/account/GetUserSettings");
				var setS = await getSetRes.Content.ReadAsStringAsync();
				if (!getSetRes.IsSuccessStatusCode || string.IsNullOrWhiteSpace(setS) || !setS.StartsWith('{'))
				{
					return retVal;
				}
				var settings = JsonSerializer.Deserialize<StandardResponse<UserSettingsResponse>>(setS, DefaultJsonOptions.DefaultOptions);
				if (settings?.Data == null || !settings.Success)
				{
					return retVal;
				}
				retVal.ColorCardPercent = settings.Data.ColorCardPercent;
				retVal.ColorCardThreshold = settings.Data.ColorCardThreshold;
				retVal.ShowCardColorInGame = settings.Data.ShowCardColorInGame;

				var c1 = await _jsRuntime.InvokeAsync<string>("getCssVar", "--color-percent-grad-red");
				retVal.GradColors.Add(new Color(c1));
				var c2 = await _jsRuntime.InvokeAsync<string>("getCssVar", "--color-percent-grad-yellow");
				retVal.GradColors.Add(new Color(c2));
				var c3 = await _jsRuntime.InvokeAsync<string>("getCssVar", "--color-percent-grad-green");
				retVal.GradColors.Add(new Color(c3));
				//var c4 = await _jsRuntime.InvokeAsync<string>("getCssVar", "--color-percent-grad-blue");
				//retVal.GradColors.Add(new Color(c4));
				//var c5 = await _jsRuntime.InvokeAsync<string>("getCssVar", "--color-percent-grad-purple");
				//retVal.GradColors.Add(new Color(c5));

				var cache = new LocalStorageItem<UserSettingsView>
				{
					Value = retVal,
					ExpireTimeUnix = DateTimeOffset.Now.AddMinutes(5).ToUnixTimeMilliseconds()
				};
				await _jsRuntime.InvokeVoidAsync("setLocalStorageCache", "usersettings", JsonSerializer.Serialize(cache, DefaultJsonOptions.DefaultOptions));
			}
			else
				retVal = cacheSettings;


			return retVal;
		}
	}
}
