using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SeedHealthApp.Models;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Essentials;

namespace SeedHealthApp.ViewModels
{
    public class ViewModelBase : BindableBase, IInitialize, INavigationAware, IDestructible
    {
        protected ISettingsService SettingsService { get; private set; }
        protected INavigationService NavigationService { get; private set; }
        protected IPageDialogService PageDialogService { get; private set; }
        protected IEventAggregator EventAggregator { get; private set; }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                SetProperty(ref _isBusy, value);
                RaisePropertyChanged(nameof(IsIdle));
            }
        }
        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }
        private bool _isOffline;
        public bool IsOffline
        {
            get { return _isOffline; }
            set { SetProperty(ref _isOffline, value); }
        }
        private bool _isModeOffline;
        public bool IsModeOffline
        {
            get { return _isModeOffline; }
            set { SetProperty(ref _isModeOffline, value); }
        }
        private ObservableCollection<BreadcumItem> _navigationUriList;
        public ObservableCollection<BreadcumItem> NavigationUriList
        {
            get { return _navigationUriList; }
            set { SetProperty(ref _navigationUriList, value); }
        }
        public bool IsIdle { get => !_isBusy; }
        public bool ShowDebugInfo =>
#if DEBUG
                true;
#else
                false;
#endif
        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetProperty(ref _isLoaded, value); }
        }

        public ViewModelBase(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IEventAggregator eventAggregator)
        {
            NavigationService = navigationService;
            PageDialogService = pageDialogService;
            SettingsService = settingsService;
            EventAggregator = eventAggregator;

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            IsOffline = Connectivity.NetworkAccess != NetworkAccess.Internet;

            NavigationUriList = new ObservableCollection<BreadcumItem>();
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            IsOffline = e.NetworkAccess != NetworkAccess.Internet;
        }

        public virtual void InitializeAsync(INavigationParameters parameters)
        {

        }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {

        }

        public virtual void Destroy()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        public virtual void Initialize(INavigationParameters parameters)
        {

        }
    }
}
