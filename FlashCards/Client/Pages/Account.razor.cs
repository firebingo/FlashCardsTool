using FlashCards.Client.Components;
using FlashCards.Shared.Models;
using FlashCards.Shared.Models.Auth;
using FlashCards.Shared.Util;
using FlashCards.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlashCards.Client.Pages
{
	public partial class Account : ComponentBase
	{
		private UserHeaderInfoResponse _userHeaderInfo = new UserHeaderInfoResponse();
		private UserSettingsView _userSettings = new UserSettingsView();
		private SettingsPageView _settings = new SettingsPageView();
		private EditContext? _passwordEditContext;
		private EditContext? _settingsEditContext;

		private bool _loading = true;
		private bool _loadingPasswordChange = false;
		private bool _loadingSettingsChange = false;
		private string _errorMessage = string.Empty;
		private string _passwordErrorMessage = string.Empty;
		private string _passwordSuccessMessage = string.Empty;
		private string _settingsErrorMessage = string.Empty;
		private string _settingsSuccessMessage = string.Empty;
		private bool _hasSettingsChange = false;

		public bool PasswordValid
		{
			get => PasswordModel != null && !string.IsNullOrWhiteSpace(PasswordModel.OldPassword) &&
				!string.IsNullOrWhiteSpace(PasswordModel.NewPassword) && !string.IsNullOrWhiteSpace(PasswordModel.NewPasswordConfirm);
		}

		public AccountPagePasswordForm? PasswordModel { get; set; }
		public AccountPageSettingsForm? SettingsModel { get; set; }

		protected override async Task OnInitializedAsync()
		{
			try
			{
				var userSettingsTask = _userSettingsService.GetUserSettings();
				using var infoRes = await _httpClient.GetAsync("/api/Account/GetUserHeaderInfo");
				var infoResS = await infoRes.Content.ReadAsStringAsync();
				if (!infoRes.IsSuccessStatusCode || string.IsNullOrWhiteSpace(infoResS) || !infoResS.StartsWith('{'))
				{
					_errorMessage = "Error loading settings.";
					return;
				}
				var info = JsonSerializer.Deserialize<StandardResponse<UserHeaderInfoResponse>>(infoResS, DefaultJsonOptions.DefaultOptions);
				if (info?.Success != true || info.Data == null)
				{
					_errorMessage = "Error loading settings.";
					return;
				}
				_userHeaderInfo = info.Data;
				_userSettings = await userSettingsTask;
				_settings = new SettingsPageView()
				{
					Username = _userHeaderInfo.UserName,
					Email = _userHeaderInfo.Email ?? string.Empty,
					ColorCardPercent = _userSettings.ColorCardPercent,
					ColorCardThreshold = _userSettings.ColorCardThreshold,
					ShowCardColorInGame = _userSettings.ShowCardColorInGame
				};

				PasswordModel ??= new AccountPagePasswordForm();
				_passwordEditContext = new EditContext(PasswordModel);

				SettingsModel ??= new AccountPageSettingsForm()
				{
					ColorCardPercent = _userSettings.ColorCardPercent,
					ColorCardThreshold = _userSettings.ColorCardThreshold,
					ShowCardColorInGame = _userSettings.ShowCardColorInGame
				};
				_settingsEditContext = new EditContext(SettingsModel);
			}
			catch
			{
				_errorMessage = "Error loading settings.";
			}
			finally
			{
				_loading = false;
				StateHasChanged();
			}
		}

		private async Task PasswordSubmit()
		{
			_loadingPasswordChange = true;
			StateHasChanged();
			try
			{
				_passwordErrorMessage = string.Empty;
				if (!_passwordEditContext!.Validate())
				{
					return;
				}
				var postModel = new ChangePasswordRequest()
				{
					OldPassword = PasswordModel!.OldPassword,
					NewPassword = PasswordModel.NewPassword,
					NewPasswordConfirm = PasswordModel.NewPasswordConfirm,
				};
				using var content = new StringContent(JsonSerializer.Serialize(postModel, DefaultJsonOptions.DefaultOptions), Encoding.UTF8, "application/json");
				using var changeRes = await _httpClient.PatchAsync("api/Account/ChangePassword", content);
				if (!changeRes.IsSuccessStatusCode)
				{
					_passwordErrorMessage = "Password change failed, please check old password is correct.";
					return;
				}
				PasswordModel.OldPassword = string.Empty;
				PasswordModel.NewPassword = string.Empty;
				PasswordModel.NewPasswordConfirm = string.Empty;
				_passwordSuccessMessage = "Password changed";
				_ = PasswordSuccessReset();
			}
			finally
			{
				_loadingPasswordChange = false;
				//StateHasChanged();
			}
		}

		private async Task PasswordSuccessReset()
		{
			await Task.Delay(3000);
			await InvokeAsync(() =>
			{
				_passwordSuccessMessage = string.Empty;
				StateHasChanged();
			});
		}

		private void OnSettingsChange()
		{
			if (SettingsModel!.ColorCardPercent != _userSettings.ColorCardPercent ||
				SettingsModel.ColorCardThreshold != _userSettings.ColorCardThreshold ||
				SettingsModel.ShowCardColorInGame != _userSettings.ShowCardColorInGame)
			{
				_hasSettingsChange = true;
			}
			else
			{
				_hasSettingsChange = false;
			}
		}

		private async Task SettingsSubmit()
		{
			if (!_hasSettingsChange)
				return;

			_loadingSettingsChange = true;
			StateHasChanged();
			try
			{
				_settingsErrorMessage = string.Empty;
				if (!_settingsEditContext!.Validate())
				{
					return;
				}
				var postModel = new UpdateUserSettingsRequest();
				if (SettingsModel!.ColorCardPercent != _userSettings.ColorCardPercent)
					postModel.ColorCardPercent = SettingsModel.ColorCardPercent;
				if (SettingsModel!.ColorCardThreshold != _userSettings.ColorCardThreshold)
					postModel.ColorCardThreshold = SettingsModel.ColorCardThreshold;
				if (SettingsModel.ShowCardColorInGame != _userSettings.ShowCardColorInGame)
					postModel.ShowCardColorInGame = SettingsModel.ShowCardColorInGame;
				using var content = new StringContent(JsonSerializer.Serialize(postModel, DefaultJsonOptions.DefaultOptions), Encoding.UTF8, "application/json");
				using var changeRes = await _httpClient.PatchAsync("api/Account/UpdateUserSettings", content);
				if (!changeRes.IsSuccessStatusCode)
				{
					_settingsErrorMessage = $"Settings update failed ({(int)changeRes.StatusCode})";
					return;
				}
				await _jsRuntime.InvokeVoidAsync("clearLocalStorageCacheItem", "usersettings");
				_userSettings.ColorCardPercent = SettingsModel!.ColorCardPercent;
				_userSettings.ColorCardThreshold = SettingsModel.ColorCardThreshold;
				_userSettings.ShowCardColorInGame = SettingsModel.ShowCardColorInGame;
				_hasSettingsChange = false;
				_settingsSuccessMessage = "Settings updated";
				_ = SettingsSuccessReset();
			}
			finally
			{
				_loadingSettingsChange = false;
				StateHasChanged();
			}
		}

		private async Task SettingsSuccessReset()
		{
			await Task.Delay(3000);
			await InvokeAsync(() =>
			{
				_settingsSuccessMessage = string.Empty;
				StateHasChanged();
			});
		}

		public class AccountPagePasswordForm()
		{
			[Required(ErrorMessage = "Required")]
			public string OldPassword { get; set; } = string.Empty;
			[Required(ErrorMessage = "Required")]
			[NotCompare(nameof(OldPassword), ErrorMessage = "Password can not be the same")]
			public string NewPassword { get; set; } = string.Empty;
			[Required(ErrorMessage = "Required")]
			[Compare(nameof(NewPassword), ErrorMessage = "Passwords must match")]
			public string NewPasswordConfirm { get; set; } = string.Empty;
		}

		public class AccountPageSettingsForm()
		{

			public bool ColorCardPercent { get; set; }
			[Required(ErrorMessage = "Required")]
			[Range(0, 100, ErrorMessage = "Must be between 0-100")]
			public int ColorCardThreshold { get; set; }
			public bool ShowCardColorInGame { get; set; }
		}
	}
}
