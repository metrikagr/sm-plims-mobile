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
            //IUserService userService = ContainerLocator.Container.Resolve<IUserService>();

#if DEBUG
            //settingsService.Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodHRwczovL2FwaS5leGFtcGxlLmNvbSIsImF1ZCI6Imh0dHBzOi8vZnJvbnRlbmQuZXhhbXBsZS5jb20iLCJqdGkiOiJQRVRFUiBSVUxFWiIsImlhdCI6MTY1NTIxMjA3MS4xODg5NDksImV4cCI6MTY4Njc0ODA3MS4xODg5NDksIm5iZiI6MTY1NTIxMjA3MS4xODg5NDksInVpZCI6MX0.fC9j4VVPbXsVeTv2Xw7se1G0YgunhffCDGTlPrEvSn0";
            //settingsService.Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodHRwczovL2FwaS5leGFtcGxlLmNvbSIsImF1ZCI6Imh0dHBzOi8vZnJvbnRlbmQuZXhhbXBsZS5jb20iLCJqdGkiOiJhQm4yajNGdXlOeDducGZrTHdtZVdFNnRrR3JxS2FLRCIsImlhdCI6MTY3ODQzODcyOS4zNDg5NzQsImV4cCI6MTcxMDA2MTEyOS4zNDg5NzQsIm5iZiI6MTY3ODQzODcyOS4zNDg5NzQsInVpZCI6MTJ9.Nn2QP8uyjKVTtztRpLrrHhJ2oonMC_jZTttmOqZL2Xc";
            //settingsService.Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodHRwczovL2FwaS5leGFtcGxlLmNvbSIsImF1ZCI6Imh0dHBzOi8vZnJvbnRlbmQuZXhhbXBsZS5jb20iLCJqdGkiOiJhQm4yajNGdXlOeDducGZrTHdtZVdFNnRrR3JxS2FLRCIsImlhdCI6MTY3ODQzODkzNi42Njc2NywiZXhwIjoxNzEwMDYxMzM2LjY2NzY3LCJuYmYiOjE2Nzg0Mzg5MzYuNjY3NjcsInVpZCI6MTF9.Dm3sIfiJHI1No2gFJ-K3LnlYCIYkwSz3I_eBqDOgblg";
            //settingsService.Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodHRwczovL2FwaS5leGFtcGxlLmNvbSIsImF1ZCI6Imh0dHBzOi8vZnJvbnRlbmQuZXhhbXBsZS5jb20iLCJqdGkiOiJhQm4yajNGdXlOeDducGZrTHdtZVdFNnRrR3JxS2FLRCIsImlhdCI6MTY3ODUwMjgxMS4xMDM2NDgsImV4cCI6MTcxMDEyNTIxMS4xMDM2NDgsIm5iZiI6MTY3ODUwMjgxMS4xMDM2NDgsInVpZCI6Mn0.kGUdDvrK-dxYbT7DWL99LPkiKm4wUdJs9-VaEDd88Eg";
            //settingsService.Username = "ivanpe834"; //admin@ebsproject.org|ivanpe834|peter.jurope|operator.plims
            //settingsService.ExpiresAt = System.DateTime.UtcNow.AddDays(1);
            //settingsService.ServerUrl = "http://34.125.38.1/ebs-plims-dev";
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
                var navResult = await NavigationService.NavigateAsync("InitialTasksPage");

                //var navigationResult = await NavigationService.NavigateAsync("HomeFlyoutPage/NavigationPage/RequestsPage");
                //if (!navigationResult.Success)
                //{
                //    System.Diagnostics.Debugger.Break();
                //}
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
            containerRegistry.Register<IUserService, UserService>();

            containerRegistry.RegisterDialog<PathogenPickerDialog, PathogenPickerDialogViewModelcs>();
            containerRegistry.RegisterDialog<RequestPickerDialog, RequestPickerDialogViewModel>();
            containerRegistry.RegisterDialog<MessageDialog, MessageDialogViewModel>();
            containerRegistry.RegisterDialog<ServerSettingsDialog, ServerSettingsDialogViewModel>();
            containerRegistry.RegisterDialog<AddRequestProcessAssaySampleTypeDialog, AddRequestProcessAssaySampleTypeDialogViewModel>();
            containerRegistry.RegisterDialog<SamplePickerDialog, SamplePickerDialogViewModel>();

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
            containerRegistry.RegisterForNavigation<MultiRequestsPage, MultiRequestsPageViewModel>();
            containerRegistry.RegisterForNavigation<MultiRequestPage, MultiRequestPageViewModel>();
            containerRegistry.RegisterForNavigation<ElisaPlateDistributionPage, ElisaPlateDistributionPageViewModel>();
            containerRegistry.RegisterForNavigation<InitialTasksPage, InitialTasksPageViewModel>();
        }
        protected override void OnStart()
        {
            
        }
    }

}
