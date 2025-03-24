using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeedHealthApp.ViewModels
{
    public class ServerSettingsPageViewModel : ViewModelBase
    {
        private string _serverUrl;
        public string ServerUrl
        {
            get { return _serverUrl; }
            set { SetProperty(ref _serverUrl, value); }
        }
        private string _host;
        public string Host
        {
            get { return _host; }
            set 
            {
                SetProperty(ref _host, value);
                OnRefreshServerUrlCommand();
            }
        }
        private string _port;
        public string Port
        {
            get { return _port; }
            set 
            {
                SetProperty(ref _port, value);
                OnRefreshServerUrlCommand();
            }
        }
        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                SetProperty(ref _path, value);
                OnRefreshServerUrlCommand();
            }
        }
        private bool _useSSL;
        public bool UseSSL
        {
            get { return _useSSL; }
            set
            {
                SetProperty(ref _useSSL, value);
                OnRefreshServerUrlCommand();
            }
        }
        private bool _useDefaultPort;
        public bool UseDefaultPort
        {
            get { return _useDefaultPort; }
            set
            {
                SetProperty(ref _useDefaultPort, value);
                OnRefreshServerUrlCommand();
            }
        }
        public ServerSettingsPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IEventAggregator eventAggregator)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            Title = "Server Settings";

            SaveCommand = new DelegateCommand(OnSaveCommand);
        }
        public DelegateCommand RefreshServerUrlCommand { get; }
        private void OnRefreshServerUrlCommand()
        {
            if(!IsBusy)
            {
                ServerUrl = $"http{(_useSSL ? "s" : "")}://{_host}{((!_useDefaultPort && !string.IsNullOrEmpty(Port)) ? ":" + Port : "")}{Path}";
            }
        }
        public DelegateCommand SaveCommand { get; }
        private async void OnSaveCommand()
        {
            try
            {
                ServerUrl = ServerUrl.TrimEnd(new char[] { '/' });
                if (Uri.IsWellFormedUriString(ServerUrl, UriKind.Absolute))
                {
                    SettingsService.ServerUrl = ServerUrl;
                    _ = await NavigationService.GoBackAsync();
                }
                else
                {
                    throw new Exception("Invalid URL");
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public override async void Initialize(INavigationParameters parameters)
        {
            try
            {
                IsBusy = true;
                if (!string.IsNullOrEmpty(SettingsService.ServerUrl))
                {
                    ServerUrl = SettingsService.ServerUrl;

                    if (Uri.IsWellFormedUriString(ServerUrl, UriKind.Absolute))
                    {
                        Uri baseUri = new Uri(ServerUrl);
                        if (!baseUri.Scheme.Equals("http") && !baseUri.Scheme.Equals("https"))
                            throw new Exception("Invalid Schema");

                        UseSSL = baseUri.Scheme.Equals("https");
                        Host = baseUri.Host;
                        Path = baseUri.AbsolutePath;
                        UseDefaultPort = baseUri.IsDefaultPort;
                        Port = baseUri.Port.ToString();
                    }
                    else
                    {
                        throw new Exception("Invalid URL");
                    }
                }
                else
                {
                    UseSSL = true;
                    UseDefaultPort = true;
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
    }
}
