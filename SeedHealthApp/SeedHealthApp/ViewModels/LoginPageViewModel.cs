using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Prism.Services.Dialogs;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeedHealthApp.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly IDeviceService _deviceService;
        public string AppVersion
        {
            get { return Xamarin.Essentials.AppInfo.VersionString; }
        }

        private string _loginMessage;
        public string LoginMessage
        {
            get { return _loginMessage; }
            set { _ = SetProperty(ref _loginMessage, value); }
        }

        public LoginPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IDialogService dialogService, IDeviceService deviceService, IEventAggregator eventAggregator)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _dialogService = dialogService;
            _settingsService = settingsService;
            _deviceService = deviceService;
            
            LoginCommand = new DelegateCommand(OnLoginCommandAsync).ObservesCanExecute(() => IsLoginEnabled);
            OpenSettingsCommand = new DelegateCommand(OnOpenSettingsCommand);

            IsLoginEnabled = CanLogin();
        }
        private bool _isLoginEnabled;
        public bool IsLoginEnabled
        {
            get { return _isLoginEnabled; }
            set { SetProperty(ref _isLoginEnabled, value); }
        }

        public bool CanLogin()
        {
            var canLogin = _settingsService.ServerUrl != null && !string.IsNullOrEmpty(_settingsService.ServerUrl);
            LoginMessage = !canLogin ? "Server is not defined" : string.Empty;
            return canLogin;
        }
        public DelegateCommand LoginCommand { get; }
        private async void OnLoginCommandAsync()
        {
            try
            {
                IsLoginEnabled = false;

                string loginUrl = $"{_settingsService.ServerUrl}/auth/auth/login-mobile";
                string callbackUrl = "fieldbook://";

                Xamarin.Essentials.WebAuthenticatorResult authenticationResult = await Xamarin.Essentials.WebAuthenticator.AuthenticateAsync(new System.Uri(loginUrl),
                                                new System.Uri(callbackUrl));

                if (!authenticationResult.Properties.ContainsKey("token") && string.IsNullOrEmpty(authenticationResult.Get("token")))
                    throw new Exception("el token está vacío");

                if (!authenticationResult.Properties.ContainsKey("username") && string.IsNullOrEmpty(authenticationResult.Get("username")))
                    throw new Exception("el usuario está vacío");

                SettingsService.Token = authenticationResult.Properties["token"];
                SettingsService.Username = authenticationResult.Properties["username"];
                long ExpiresAt = long.Parse(authenticationResult.Properties["expires_at"]);
                SettingsService.ExpiresAt = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(ExpiresAt);

                var navResult = await NavigationService.NavigateAsync("/InitialTasksPage");
                if (!navResult.Success)
                    throw navResult.Exception;
                //await NavigationService.NavigateAsync("/HomeFlyoutPage/NavigationPage/RequestsPage");
            }
            catch (TaskCanceledException)
            {
                SettingsService.Token = string.Empty;
                SettingsService.Username = string.Empty;
                SettingsService.ExpiresAt = DateTime.MinValue;
                IsLoginEnabled = true;
            }
            catch (Exception ex)
            {
                SettingsService.Token = string.Empty;
                SettingsService.Username = string.Empty;
                SettingsService.ExpiresAt = DateTime.MinValue;
                IsLoginEnabled = true;

                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        public DelegateCommand OpenSettingsCommand { get; }
        private async void OnOpenSettingsCommand()
        {
            try
            {
                if (_deviceService.Idiom == Xamarin.Forms.TargetIdiom.Tablet)
                {
                    var parameters = new DialogParameters
                    {
                        {"ServerUrl", _settingsService.ServerUrl }
                    };
                    var result = await _dialogService.ShowDialogAsync("ServerSettingsDialog", parameters);
                    if (result.Exception == null)
                    {
                        if (result.Parameters.ContainsKey("ServerUrl"))
                        {
                            _settingsService.ServerUrl = result.Parameters.GetValue<string>("ServerUrl");
                            IsLoginEnabled = CanLogin();
                        }
                    }
                    else
                        throw result.Exception;
                }
                else if (_deviceService.Idiom == Xamarin.Forms.TargetIdiom.Phone)
                {
                    var result = await NavigationService.NavigateAsync("ServerSettingsPage");
                    if (result.Exception != null)
                        throw result.Exception;
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                //Refresh IsLoginEnabled
                IsLoginEnabled = CanLogin();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
