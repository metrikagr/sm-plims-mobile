using Prism;
using Prism.Ioc;
using SeedHealthApp.Dialogs;
using SeedHealthApp.Services;
using SeedHealthApp.ViewModels;
using SeedHealthApp.Views;
using System.Threading.Tasks;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace SeedHealthApp
{
    public partial class App
    {
        public App(IPlatformInitializer initializer)
            : base(initializer)
        {
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            ISettingsService settingsService = ContainerLocator.Container.Resolve<ISettingsService>();

#if DEBUG
            //settingsService.Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiIsImp0aSI6IlBFVEVSIFJVTEVaIn0.eyJpc3MiOiJodHRwczpcL1wvYXBpLmV4YW1wbGUuY29tIiwiYXVkIjoiaHR0cHM6XC9cL2Zyb250ZW5kLmV4YW1wbGUuY29tIiwianRpIjoiUEVURVIgUlVMRVoiLCJpYXQiOjE2NTA5OTcxMDksImV4cCI6MTY4MjUzMzEwOSwidWlkIjoxfQ.ZNG7ehOCv--xkRO3z2irVCYU_TZaXiTyc8uruqS9WH8";
            settingsService.Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodHRwczovL2FwaS5leGFtcGxlLmNvbSIsImF1ZCI6Imh0dHBzOi8vZnJvbnRlbmQuZXhhbXBsZS5jb20iLCJqdGkiOiJQRVRFUiBSVUxFWiIsImlhdCI6MTY1NTIxMjA3MS4xODg5NDksImV4cCI6MTY4Njc0ODA3MS4xODg5NDksIm5iZiI6MTY1NTIxMjA3MS4xODg5NDksInVpZCI6MX0.fC9j4VVPbXsVeTv2Xw7se1G0YgunhffCDGTlPrEvSn0";
            settingsService.Username = "admin@ebsproject.org";
            settingsService.ExpiresAt = System.DateTime.UtcNow.AddDays(1);
            settingsService.ServerUrl = "http://34.125.39.167/ebs-plims-dev";
#endif

            if (string.IsNullOrEmpty(settingsService.ServerUrl))
            {
                try
                {
                    //Get default from config file
                    using (var stream = await Xamarin.Essentials.FileSystem.OpenAppPackageFileAsync("platform_config.json"))
                    {
                        using (var reader = new System.IO.StreamReader(stream))
                        {
                            var json = await reader.ReadToEndAsync();
                            var platformConfig = System.Text.Json.JsonSerializer.Deserialize<Models.PlatformConfiguration>(json);
                            settingsService.ServerUrl = platformConfig.DefaultBaseUrl;
                        }
                    }
                }
                catch
                {
                }
            }

            if (!string.IsNullOrEmpty(settingsService.Token)
                && System.DateTime.UtcNow < settingsService.ExpiresAt)
            {
                //await NavigationService.NavigateAsync("NavigationPage/RequestsPage");
                var navigationResult = await NavigationService.NavigateAsync("HomeFlyoutPage/NavigationPage/RequestsPage");
                if (!navigationResult.Success)
                {

                }
            }
            else
            {
                settingsService.ExpiresAt = System.DateTime.MinValue;
                settingsService.Token = string.Empty;
                settingsService.Username = string.Empty;

                await NavigationService.NavigateAsync("NavigationPage/LoginPage");
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();

            containerRegistry.Register<IRequestService, RequestService>();
            containerRegistry.Register<ISettingsService, SettingsService>();
            containerRegistry.Register<IRequestProcessAssayActivitySampleRepository, RequestProcessAssayActivitySampleRepository>();
            containerRegistry.Register<IRequestProcessAssayRepository, RequestProcessAssayRepository>();
            containerRegistry.Register<IResultRepository, ResultRepository>();
            containerRegistry.Register<IToastService, ToastService>();

            containerRegistry.RegisterDialog<PathogenPickerDialog, PathogenPickerDialogViewModelcs>();
            containerRegistry.RegisterDialog<RequestPickerDialog, RequestPickerDialogViewModel>();
            containerRegistry.RegisterDialog<MessageDialog, MessageDialogViewModel>();
            containerRegistry.RegisterDialog<ServerSettingsDialog, ServerSettingsDialogViewModel>();
            containerRegistry.RegisterDialog<AddRequestProcessAssaySampleTypeDialog,AddRequestProcessAssaySampleTypeDialogViewModel>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<RequestsPage, RequestsPageViewModel>(
                tabletView: typeof(TabletRequestsPage));
            containerRegistry.RegisterForNavigationOnIdiom<RequestPage, RequestPageViewModel>(
                tabletView: typeof(TabletRequestPage));
            containerRegistry.RegisterForNavigationOnIdiom<AssayGroupPage, AssayGroupPageViewModel>(
                tabletView: typeof(TabletAssayGroupPage));
            containerRegistry.RegisterForNavigationOnIdiom<AssayPage, AssayPageViewModel>(
                tabletView: typeof(TabletAssayPage));
            containerRegistry.RegisterForNavigation<TabletRequestsPage, RequestsPageViewModel>();
            containerRegistry.RegisterForNavigation<TabletRequestPage, RequestPageViewModel>();
            containerRegistry.RegisterForNavigation<TabletAssayGroupPage, AssayGroupPageViewModel>();
            containerRegistry.RegisterForNavigationOnIdiom<AssayPreparationPage, AssayPreparationPageViewModel>(
                tabletView: typeof(TabletAssayPreparationPage));
            containerRegistry.RegisterForNavigationOnIdiom<ResultsBlotterPage, ResultsBlotterPageViewModel>(
                tabletView: typeof(TabletResultsBlotterPage));
            containerRegistry.RegisterForNavigationOnIdiom<ResultsGerminationTestPage, ResultsGerminationTestPageViewModel>(
                tabletView: typeof(TabletResultsGerminationTestPage));
            containerRegistry.RegisterForNavigationOnIdiom<ResultsPcrPage, ResultsPcrPageViewModel>(
                tabletView: typeof(TabletResultsPcrPage));
            containerRegistry.RegisterForNavigation<ResultsElisaPage, ResultsElisaPageViewModel>();
            containerRegistry.RegisterForNavigation<ElisaDistributionPage, ElisaDistributionPageViewModel>();
            containerRegistry.RegisterForNavigation<ServerSettingsPage, ServerSettingsPageViewModel>();
            
            containerRegistry.RegisterForNavigation<HomeFlyoutPage, HomeFlyoutPageViewModel>();
        }
        protected override void OnStart()
        {
            
        }
    }

}
