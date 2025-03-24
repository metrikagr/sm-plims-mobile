using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SeedHealthApp.Extensions;
using SeedHealthApp.Helpers;
using SeedHealthApp.Models;
using SeedHealthApp.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeedHealthApp.ViewModels
{
    public class RequestsPageViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;

        private IEnumerable<Request> _requestList;
        public IEnumerable<Request> RequestList
        {
            get { return _requestList; }
            set { SetProperty(ref _requestList, value); }
        }

        private Request _selectedRequest;
        public Request SelectedRequest
        {
            get { return _selectedRequest; }
            set { SetProperty(ref _selectedRequest, value); }
        }
        private bool _isListEmpty;
        public bool IsListEmpty
        {
            get { return _isListEmpty; }
            set { SetProperty(ref _isListEmpty, value); }
        }
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
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set { SetProperty(ref _isRefreshing, value); }
        }
        private MultiRequest _selectedMultiRequest;
        public MultiRequest SelectedMultiRequest
        {
            get { return _selectedMultiRequest; }
            set { SetProperty(ref _selectedMultiRequest, value); }
        }
        public RequestsPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IRequestService requestService, IEventAggregator eventAggregator) 
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _requestService = requestService;

            Title = "Requests";
            Username = SettingsService.Username;

            GetEssaysCommand = new DelegateCommand(OnGetEssaysCommand).ObservesCanExecute(() => IsIdle);

            CreateMultiRequestCommand = new DelegateCommand(ExecCreateMultiRequestCommand).ObservesCanExecute(() => IsIdle);
            RefreshMultiRequestCommand = new DelegateCommand(OnRefreshMultiRequestCommand).ObservesCanExecute(() => IsIdle);
            MultirequestSelectedCommand = new DelegateCommand(ExecuteMultirequestSelectedCommand).ObservesCanExecute(() => IsIdle);

            NavigationUriList.Add(new BreadcumItem
            {
                Order = 1,
                Title = "Requests",
                NavigationUri = nameof(RequestsPageViewModel),
                IsFirst = true
            });
        }
        private DelegateCommand _refreshCommand;
        public DelegateCommand RefreshCommand =>
            _refreshCommand ?? (_refreshCommand = new DelegateCommand(ExecuteRefreshCommand).ObservesCanExecute(() => IsIdle));

        private async void ExecuteRefreshCommand()
        {
            try
            {
                IsBusy = true;
                IsRefreshing = true;

                RequestList = await _requestService.GetRequests(SettingsService.LaboratoryId);
                IsListEmpty = !RequestList.Any();

                IsRefreshing = false;
                IsBusy = false;
            }
            catch (Exception ex)
            {
                RequestList = Enumerable.Empty<Request>();
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
            finally
            {
                IsRefreshing = false;
                IsBusy = false;
            }
        }
        public DelegateCommand GetEssaysCommand { get; }
        private async void OnGetEssaysCommand()
        {
            try
            {
                IsBusy = true;
                if (SelectedRequest != null)
                {
                    INavigationParameters nagigationParemeters = new NavigationParameters()
                    {
                        { "Request", SelectedRequest }
                    };
                    await NavigationService.NavigateAsync("RequestPage", nagigationParemeters);
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

                    var selectedElisaRequestList = elisaEvent.request_process.Select(x => new RequestLookup()
                    {
                        request_id = x.request_id,
                        request_code = x.request_code,
                        process_id = x.process_id
                    }).ToArray();

                    //var apiResponse = await _requestService.GetElisaRequestsByAgents(elisaEvent.agents);
                    //if (apiResponse.Status == 200)
                    //{
                    //    foreach (var requestLookup in apiResponse.Data.OrderByDescending(x => x.request_id))
                    //    {
                    //        if(elisaEvent.request_process.Any(x => x.request_id == requestLookup.request_id && x.process_id == requestLookup.process_id))
                    //            selectedElisaRequestList.Add(requestLookup);
                    //    }
                    //}
                    //else
                    //    throw new Exception(pathogenApiResponse.Message);

                    var platesDistributions = await _requestService.GetSupportLocationsAsync(elisaEvent.event_id);

                    INavigationParameters navigationParameters = new NavigationParameters {
                        { "SelectedRequestList", selectedElisaRequestList},
                        { "SelectedPathogenList", selectedPathogenItemList},
                        { "ElisaEvent", elisaEvent },
                    };
                    if (platesDistributions.Any())
                        navigationParameters.Add("PlatesDistributions", platesDistributions);

                    var result = await NavigationService.NavigateAsync("ElisaDistributionPage", navigationParameters);
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

        public override async void Initialize(INavigationParameters parameters)
        {
            try
            {
                IsBusy = true;

                RequestList = await _requestService.GetRequests(SettingsService.LaboratoryId);
                IsListEmpty = !RequestList.Any();
            }
            catch (Exception ex)
            {
                RequestList = Enumerable.Empty<Request>();
                await PageDialogService.DisplayErrorAlertAsync(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
        //public override async void OnNavigatedTo(INavigationParameters parameters)
        //{
        //    try
        //    {
        //        IsBusy = false;
        //        if (parameters.ContainsKey("RefreshMultiRequests"))
        //        {
        //            IsMultiRequestListRefreshing = true;

        //            var tempMultiRequests = await _requestService.GetMultiRequests();
        //            MultiRequestList = tempMultiRequests;
        //            if (!MultiRequestList.Any())
        //                IsMultiRequestListEmpty = true;
        //            else
        //                IsMultiRequestListEmpty = false;

        //            IsMultiRequestListRefreshing = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await PageDialogService.DisplayErrorAlertAsync(ex);
        //    }
        //    finally
        //    {
        //        IsMultiRequestListRefreshing = false;
        //        IsBusy = false;
        //    }
        //}
    }
}
