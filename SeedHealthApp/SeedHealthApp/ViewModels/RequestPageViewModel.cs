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
using System.Linq;
using System.Threading.Tasks;

namespace SeedHealthApp.ViewModels
{
    public class RequestPageViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;

        private Request _request;
        public Request Request
        {
            get { return _request; }
            set { SetProperty(ref _request, value); }
        }
        private IEnumerable<Assay> _essayList;
        public IEnumerable<Assay> EssayList
        {
            get { return _essayList; }
            set { SetProperty(ref _essayList, value); }
        }
        private Assay _selectedEssay;
        public Assay SelectedEssay
        {
            get { return _selectedEssay; }
            set { SetProperty(ref _selectedEssay, value); }
        }
        private bool _isListEmpty;
        public bool IsListEmpty
        {
            get { return _isListEmpty; }
            set { SetProperty(ref _isListEmpty, value); }
        }
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set { SetProperty(ref _isRefreshing, value); }
        }
        private IEnumerable<Assay> _reprocessAssayList;
        public IEnumerable<Assay> ReprocessAssayList
        {
            get { return _reprocessAssayList; }
            set { SetProperty(ref _reprocessAssayList, value); }
        }
        private Assay _selectedReprocessAssay;
        public Assay SelectedReprocessAssay
        {
            get { return _selectedReprocessAssay; }
            set { SetProperty(ref _selectedReprocessAssay, value); }
        }

        public RequestPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IRequestService requestService, IEventAggregator eventAggregator)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _requestService = requestService;

            OpenEssaySettingsCommand = new DelegateCommand(OnOpenEssaySettingsCommand).ObservesCanExecute(() => IsIdle);
            RefreshCommand = new DelegateCommand(ExecuteRefreshCommand).ObservesCanExecute(() => IsIdle);
            OpenReprocessAssayCommand = new DelegateCommand(ExecuteOpenReprocessAssayCommand).ObservesCanExecute(() => IsIdle);

            Title = "Manage Request";
            Username = SettingsService.Username;

            NavigationUriList = new ObservableCollection<BreadcumItem>()
            {
                new BreadcumItem { Order = 1, Title = "Requests", NavigationUri = nameof(RequestsPageViewModel), IsFirst = true },
                //new BreadcumItem { Order = 1, Title = "Requests", NavigationUri = nameof(RequestsPageViewModel) }
            };
        }
        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand OpenEssaySettingsCommand { get; }
        private async void OnOpenEssaySettingsCommand()
        {
            try
            {
                IsBusy = true;
                if (SelectedEssay != null)
                {
                    if (SelectedEssay.assay_id == (int)Models.AssayMobileEnum.Elisa)
                        return;

                    INavigationParameters nagigationParemeters = new NavigationParameters()
                    {
                        { "Request", Request },
                        { "Essay", SelectedEssay },
                    };
                    var navigationResult = await NavigationService.NavigateAsync("AssayGroupPage", nagigationParemeters);
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "Ok");
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async void ExecuteRefreshCommand()
        {
            try
            {
                IsBusy = true;
                IsRefreshing = true;

                await LoadAssays();

                IsRefreshing = false;
                IsBusy = false;
            }
            catch (Exception ex)
            {
                EssayList = Enumerable.Empty<Assay>();
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
            finally
            {
                IsRefreshing = false;
                IsBusy = false;
            }
        }
        public DelegateCommand OpenReprocessAssayCommand { get; }
        private async void ExecuteOpenReprocessAssayCommand()
        {
            try
            {
                IsBusy = true;
                if (SelectedReprocessAssay != null)
                {
                    if (SelectedReprocessAssay.assay_id == (int)AssayMobileEnum.Elisa)
                        return;

                    INavigationParameters nagigationParemeters = new NavigationParameters()
                    {
                        { "Request", Request },
                        { "Essay", SelectedReprocessAssay }
                    };
                    var navigationResult = await NavigationService.NavigateAsync("AssayGroupPage", nagigationParemeters);
                    if (!navigationResult.Success)
                        throw navigationResult.Exception;
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "Ok");
            }
            finally
            {
                IsBusy = false;
            }
        }
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                IsBusy = true;

                if (parameters.ContainsKey("Request"))
                {
                    Request = (Request)parameters["Request"];

                    Title = $"Manage Request ({_request.request_code})";
                    NavigationUriList.Add(new BreadcumItem
                    {
                        Order = 2,
                        Title = Title,
                        NavigationUri = nameof(RequestPageViewModel)
                    });

                    await LoadAssays();
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "Ok");
            }
        }

        private async Task LoadAssays()
        {
            var tempAssayList = await _requestService.GetAssayHeaders(_request.request_id, 1);
            if (!tempAssayList.Any())
                IsListEmpty = true;
            else
            {
                EssayList = tempAssayList;
                //EssayList = tempAssayList.Where(x => x.process_id == 1).ToArray();
                //ReprocessAssayList = tempAssayList.Where(x => x.process_id > 1).ToArray();
            }
        } 
    }
}
