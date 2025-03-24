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
    public class InitialTasksPageViewModel : ViewModelBase
    {
        private readonly IUserService _userService;
        public InitialTasksPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService,
            ISettingsService settingsService, IEventAggregator eventAggregator, IUserService userService)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _userService = userService;
        }
        public override async void Initialize(INavigationParameters parameters)
        {
            try
            {
                var userInfo = await _userService.GetUserInfoAsync();

                SettingsService.RoleId = userInfo.role_id ?? 0;
                SettingsService.Role = userInfo.role_name;
                SettingsService.InstitutionId = userInfo.institution_id ?? 0;
                SettingsService.InstitutionName = userInfo.institution_name;
                SettingsService.LaboratoryId = userInfo.laboratory_id ?? 0;
                SettingsService.LaboratoryName = userInfo.laboratory_name;

                var navigationResult = await NavigationService.NavigateAsync("/HomeFlyoutPage/NavigationPage/RequestsPage");
                if (!navigationResult.Success)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
    }
}
