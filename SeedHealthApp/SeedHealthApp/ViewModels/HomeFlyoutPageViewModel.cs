using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SeedHealthApp.Models;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeedHealthApp.ViewModels
{
    public class HomeFlyoutPageViewModel : ViewModelBase
    {
        public string AppVersion
        {
            get { return Xamarin.Essentials.AppInfo.VersionString; }
        }
        private IEnumerable<NavigationMenuItem> _navigationMenuItemList;
        public IEnumerable<NavigationMenuItem> NavigationMenuItemList
        {
            get { return _navigationMenuItemList; }
            set { SetProperty(ref _navigationMenuItemList, value); }
        }
        private NavigationMenuItem _selectedMenuItem;
        public NavigationMenuItem SelectedMenuItem
        {
            get { return _selectedMenuItem; }
            set { SetProperty(ref _selectedMenuItem, value); }
        }
        public string Role { get => SettingsService.Role; }
        public string InstitutionName { get => SettingsService.InstitutionName; }
        public string LaboratoryName { get => SettingsService.LaboratoryName; }

        public HomeFlyoutPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IEventAggregator eventAggregator)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            Username = SettingsService.Username;

            NavigationMenuItemList = new List<NavigationMenuItem>()
            {
                new NavigationMenuItem{ Text = "Requests", Path = "RequestsPage" },
                new NavigationMenuItem{ Text = "Multi requests", Path = "MultiRequestsPage" },
            };
            SelectedMenuItem = NavigationMenuItemList.First();

            ItemTappedCommand = new DelegateCommand(ExecuteItemTappedCommand).ObservesCanExecute(() => IsIdle);
            NavigateCommand = new DelegateCommand<string>(OnNavigateCommand);
            LogoutCommand = new DelegateCommand(OnLogoutCommand);
        }
        public DelegateCommand ItemTappedCommand { get; }
        private async void ExecuteItemTappedCommand()
        {
            try
            {
                IsBusy = true;
                var result = await NavigationService.NavigateAsync("/HomeFlyoutPage/NavigationPage/" + SelectedMenuItem.Path);
                if (!result.Success)
                    throw result.Exception;
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        public DelegateCommand<string> NavigateCommand { get; }
        private async void OnNavigateCommand(string pageName)
        {
            try
            {
                //await PageDialogService.DisplayAlertAsync("",NavigationService.GetNavigationUriPath(),"OK");
                var result = await NavigationService.NavigateAsync("/HomeFlyoutPage/NavigationPage/" + pageName);
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand LogoutCommand { get; }
        private async void OnLogoutCommand()
        {
            try
            {
                SettingsService.ExpiresAt = System.DateTime.MinValue;
                SettingsService.Token = string.Empty;
                SettingsService.Username = string.Empty;

                SettingsService.RoleId = 0;
                SettingsService.Role = string.Empty;
                SettingsService.InstitutionId = 0;
                SettingsService.InstitutionName = string.Empty;
                SettingsService.LaboratoryId = 0;
                SettingsService.LaboratoryName = string.Empty;

                await NavigationService.NavigateAsync("/NavigationPage/LoginPage");
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
    }
}
