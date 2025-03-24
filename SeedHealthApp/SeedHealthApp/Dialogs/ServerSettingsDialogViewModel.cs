using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Dialogs
{
    public class ServerSettingsDialogViewModel : BindableBase, IDialogAware
    {
        public event Action<IDialogParameters> RequestClose;
        private string _serverUrl;
        public string ServerUrl
        {
            get { return _serverUrl; }
            set { SetProperty(ref _serverUrl, value); }
        }
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                SetProperty(ref _host, value);
                JoinServerUrl();
            }
        }
        private string _port;
        public string Port
        {
            get { return _port; }
            set
            {
                SetProperty(ref _port, value);
                JoinServerUrl();
            }
        }
        private string _path;
        public string Path
        {
            get { return _path; }
            set 
            {
                SetProperty(ref _path, value);
                JoinServerUrl();
            }
        }
        private bool _useSSL;
        public bool UseSSL
        {
            get { return _useSSL; }
            set
            {
                SetProperty(ref _useSSL, value);
                JoinServerUrl();
            }
        }
        private bool _useDefaultPort;
        public bool UseDefaultPort
        {
            get { return _useDefaultPort; }
            set
            {
                SetProperty(ref _useDefaultPort, value);
                JoinServerUrl();
            }
        }
        public bool IsBusy { get; set; }
        public ServerSettingsDialogViewModel()
        {
            CloseCommand = new DelegateCommand(() => RequestClose(null));
            AcceptCommand = new DelegateCommand(OnAcceptCommand);
        }
        private void JoinServerUrl()
        {
            if (!IsBusy)
            {
                ServerUrl = $"http{(_useSSL ? "s" : "")}://{_host}{((!_useDefaultPort && !string.IsNullOrEmpty(Port)) ? ":" + Port : "")}{Path}";
                //RaisePropertyChanged(nameof(ServerUrl));
            }
        }
        
        public DelegateCommand CloseCommand { get; }
        public DelegateCommand AcceptCommand { get; }
        private void OnAcceptCommand()
        {
            try
            {
                Message = string.Empty;
                ServerUrl = ServerUrl.TrimEnd(new char[] { '/' });
                if (!Uri.IsWellFormedUriString(ServerUrl, UriKind.Absolute))
                {
                    throw new Exception("Invalid URL");
                }

                RequestClose(new DialogParameters()
                {
                    { "ServerUrl", ServerUrl }
                });
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                Console.WriteLine("Error : " + ex.Message);
            }
        }
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            try
            {
                IsBusy = true;
                
                if (parameters.ContainsKey("ServerUrl") && parameters["ServerUrl"] != null)
                {
                    ServerUrl = parameters.GetValue<string>("ServerUrl");
                    if (!string.IsNullOrEmpty(ServerUrl))
                    {
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
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                Message = ex.Message;
                Console.WriteLine(ex.Message);
            }
        }
    }
}
