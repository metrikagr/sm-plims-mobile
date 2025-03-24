using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SeedHealthApp.Extensions;
using SeedHealthApp.Models;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeedHealthApp.ViewModels
{
    public class MultiRequestsPageViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;

        private bool _isMultiRequestListEmpty;
        public bool IsMultiRequestListEmpty
        {
            get { return _isMultiRequestListEmpty; }
            set { SetProperty(ref _isMultiRequestListEmpty, value); }
        }

        private IEnumerable<MultiRequest> _multiRequestList;
        public IEnumerable<MultiRequest> MultiRequestList
        {
            get { return _multiRequestList; }
            set { SetProperty(ref _multiRequestList, value); }
        }
        private bool _isMultiRequestListRefreshing;
        public bool IsMultiRequestListRefreshing
        {
            get { return _isMultiRequestListRefreshing; }
            set { SetProperty(ref _isMultiRequestListRefreshing, value); }
        }
        private MultiRequest _selectedMultiRequest;
        public MultiRequest SelectedMultiRequest
        {
            get { return _selectedMultiRequest; }
            set { SetProperty(ref _selectedMultiRequest, value); }
        }
        public MultiRequestsPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IEventAggregator eventAggregator, IRequestService requestService)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _requestService = requestService;

            Username = SettingsService.Username;

            CreateMultiRequestCommand = new DelegateCommand(ExecCreateMultiRequestCommand).ObservesCanExecute(() => IsIdle);
            RefreshMultiRequestCommand = new DelegateCommand(OnRefreshMultiRequestCommand).ObservesCanExecute(() => IsIdle);
            MultirequestSelectedCommand = new DelegateCommand(ExecuteMultirequestSelectedCommand).ObservesCanExecute(() => IsIdle);
            //PathogenItemTappedCommand = new DelegateCommand(ExecutePathogenItemTappedCommand).ObservesCanExecute(() => IsIdle)

            NavigationUriList.Add(new BreadcumItem
            {
                Order = 1,
                Title = "Multi requests",
                NavigationUri = nameof(MultiRequestsPageViewModel),
                IsFirst = true
            });
        }

        public DelegateCommand CreateMultiRequestCommand { get; }
        private async void ExecCreateMultiRequestCommand()
        {
            try
            {
                IsBusy = true;
                var navigationResult = await NavigationService.NavigateAsync("ResultsElisaPage");
                if (!navigationResult.Success)
                {
                    throw navigationResult.Exception;
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public DelegateCommand RefreshMultiRequestCommand { get; }
        private async void OnRefreshMultiRequestCommand()
        {
            try
            {
                IsMultiRequestListRefreshing = true;

                var tempMultiRequests = await _requestService.GetMultiRequests(SettingsService.LaboratoryId);
                MultiRequestList = tempMultiRequests;
                if (!MultiRequestList.Any())
                    IsMultiRequestListEmpty = true;
                else
                    IsMultiRequestListEmpty = false;

                IsMultiRequestListRefreshing = false;
            }
            catch (Exception ex)
            {
                MultiRequestList = new List<MultiRequest>();
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand MultirequestSelectedCommand { get; }
        private async void ExecuteMultirequestSelectedCommand()
        {
            try
            {
                IsBusy = true;
                if (SelectedMultiRequest != null)
                {
                    //Get elisa multirequest event data
                    var elisaEvent = await _requestService.GetEvent(_selectedMultiRequest.event_id);
                    if(string.IsNullOrEmpty(elisaEvent.event_code))
                    {
                        elisaEvent.event_code = _selectedMultiRequest.event_code;
                    }
                    if (elisaEvent.status_id == 0)
                    {
                        elisaEvent.status_id = _selectedMultiRequest.status_id;
                        elisaEvent.status_name = _selectedMultiRequest.status_name;
                    }
                    
                    var selectedPathogenItemList = new List<PathogenItem>();
                    var pathogenApiResponse = await _requestService.GetElisaAgents();
                    if (pathogenApiResponse.Status == 200)
                    {
                        foreach (var agent in pathogenApiResponse.Data)
                        {
                            if (elisaEvent.agents.Contains(agent.agent_id))
                            {
                                selectedPathogenItemList.Add(new PathogenItem()
                                {
                                    PathogenId = agent.agent_id,
                                    PathogenName = agent.agent_name
                                });
                            }
                        }
                    }
                    else
                        throw new Exception(pathogenApiResponse.Message);

                    //var selectedElisaRequestList = elisaEvent.request_process.Select(x => new RequestLookup()
                    //{
                    //    request_id = x.request_id,
                    //    request_code = x.request_code,
                    //    process_id = x.process_id
                    //}).ToArray();

                    var platesDistributions = await _requestService.GetSupportLocationsAsync(elisaEvent.event_id);

                    INavigationParameters navigationParameters = new NavigationParameters {
                        { "SelectedPathogenList", selectedPathogenItemList},
                        { "ElisaEvent", elisaEvent },
                    };
                    if (platesDistributions.Any())
                        navigationParameters.Add("PlatesDistributions", platesDistributions);

                    var result = await NavigationService.NavigateAsync("MultiRequestPage", navigationParameters);
                    //var result = await NavigationService.NavigateAsync("ElisaDistributionPage", navigationParameters);
                    if (!result.Success)
                    {
                        throw result.Exception;
                    }
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        //public DelegateCommand PathogenItemTappedCommand { get; }
        //private async void ExecutePathogenItemTappedCommand()
        //{
        //    try
        //    {
        //        IsBusy = true;


        //    }
        //    catch (Exception ex)
        //    {
        //        await PageDialogService.DisplayErrorAlertAsync(ex);
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //    }
        //}
        public override async void Initialize(INavigationParameters parameters)
        {
            try
            {
                IsBusy = true;
                
                MultiRequestList = await _requestService.GetMultiRequests(SettingsService.LaboratoryId);
                IsMultiRequestListEmpty = !MultiRequestList.Any();
            }
            catch (Exception ex)
            {
                MultiRequestList = Enumerable.Empty<MultiRequest>();
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
                IsBusy = false;
                if (parameters.ContainsKey("RefreshMultiRequests"))
                {
                    IsMultiRequestListRefreshing = true;

                    MultiRequestList = await _requestService.GetMultiRequests(SettingsService.LaboratoryId);
                    IsMultiRequestListEmpty = !MultiRequestList.Any();

                    IsMultiRequestListRefreshing = false;
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayErrorAlertAsync(ex);
            }
            finally
            {
                IsMultiRequestListRefreshing = false;
                IsBusy = false;
            }
        }
    }
}
