using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Prism.Services.Dialogs;
using SeedHealthApp.Models;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SeedHealthApp.ViewModels
{
    public class ResultsElisaPageViewModel : ViewModelBase
    {
        IRequestService _requestService;
        IDialogService _dialogService;
        IEnumerable<int> _selectedPathogenIds = new List<int>();

        private bool _isDataVisible;
        public bool IsDataVisible
        {
            get { return _isDataVisible; }
            set { SetProperty(ref _isDataVisible, value); }
        }
        private string _entryType;
        public string EntryType
        {
            get { return _entryType; }
            set { SetProperty(ref _entryType, value); }
        }
        private IEnumerable<PlateLocation> _locationList;
        public IEnumerable<PlateLocation> LocationList
        {
            get { return _locationList; }
            set { SetProperty(ref _locationList, value); }
        }
        private IEnumerable<PathogenItem> _pathogenList;
        public IEnumerable<PathogenItem> PathogenList
        {
            get { return _pathogenList; }
            set { SetProperty(ref _pathogenList, value); }
        }
        private Request _request;
        public Request Request
        {
            get { return _request; }
            set { SetProperty(ref _request, value); }
        }
        private Assay _essay;
        public Assay Essay
        {
            get { return _essay; }
            set { SetProperty(ref _essay, value); }
        }

        private IEnumerable<SelectableModel<RequestLookup>> _elisaRequestList;
        public IEnumerable<SelectableModel<RequestLookup>> ElisaRequestList
        {
            get { return _elisaRequestList; }
            set { SetProperty(ref _elisaRequestList, value); }
        }
        private ObservableCollection<RequestLookup> _selectedRequestList;
        public ObservableCollection<RequestLookup> SelectedRequestList
        {
            get { return _selectedRequestList; }
            set { SetProperty(ref _selectedRequestList, value); }
        }
        private bool _isRequestListVisible;
        public bool IsRequestListVisible
        {
            get { return _isRequestListVisible; }
            set { SetProperty(ref _isRequestListVisible, value); }
        }
        private RequestLookup _selectedRequestLookup;
        public RequestLookup SelectedRequestLookup
        {
            get { return _selectedRequestLookup; }
            set { SetProperty(ref _selectedRequestLookup, value); }
        }

        public ResultsElisaPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IRequestService requestService, IDialogService dialogService, IEventAggregator eventAggregator) 
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _requestService = requestService;
            _dialogService = dialogService;

            Title = "Create new multirequest";
            Username = SettingsService.Username;

            IsDataVisible = false;

            SearchRequestsCommand = new DelegateCommand(OnSearchRequestsCommand);
            SetDistributionCommand = new DelegateCommand(OnSetDistributionCommand);
            RequestSelectedCommand = new DelegateCommand(OnRequestSelectedCommand);

            RemoveRequestCommand = new DelegateCommand<object>(OnRemoveRequestCommand);

            SelectedRequestList = new ObservableCollection<RequestLookup>();
        }
        public DelegateCommand SearchRequestsCommand { get; }
        private async void OnSearchRequestsCommand()
        {
            try
            {
                SelectedRequestLookup = null;

                var selectedPathogenIds = PathogenList.Where(x => x.IsSelected).Select(x => x.PathogenId);
                if (!selectedPathogenIds.Any())
                {
                    throw new Exception("Select at least one pathogen");
                }
                //Check if the pathogen selection has changed
                if (_selectedPathogenIds.Count() != selectedPathogenIds.Count() ||
                    _selectedPathogenIds.Intersect(selectedPathogenIds).Count() != selectedPathogenIds.Count())
                {
                    var apiResponse = await _requestService.GetElisaRequestsByAgents(SettingsService.LaboratoryId, selectedPathogenIds);
                    if (apiResponse.Status == 200)
                    {
                        var tempElisaRequestList = new List<SelectableModel<RequestLookup>>();
                        foreach (var requestLookup in apiResponse.Data.OrderByDescending(x => x.request_id))
                        {
                            tempElisaRequestList.Add(new SelectableModel<RequestLookup>
                            {
                                Item = requestLookup
                            });
                        }
                        ElisaRequestList = tempElisaRequestList;
                        _selectedPathogenIds = selectedPathogenIds.ToList();
                    }
                    else
                        ElisaRequestList = null;
                }

                if (ElisaRequestList == null)
                    throw new Exception("Request list is empty");

                foreach (var item in ElisaRequestList)
                {
                    item.Selected = false;
                }
                var parameters = new DialogParameters
                {
                    { "RequestList", ElisaRequestList }
                };
                var dialogResult = await _dialogService.ShowDialogAsync("RequestPickerDialog", parameters);
                if (dialogResult.Parameters.ContainsKey("SelectedRequestList"))
                {
                    var tempSelectedRequestList = dialogResult.Parameters.GetValue<IEnumerable<SelectableModel<RequestLookup>>>("SelectedRequestList");
                    foreach (var tempSelectedRequest in tempSelectedRequestList)
                    {
                        if(!SelectedRequestList.Any(x => x.request_id == tempSelectedRequest.Item.request_id))
                        {
                            SelectedRequestList.Add(tempSelectedRequest.Item);
                        }
                    }
                }

                //IsRequestListVisible = true;
            }
            catch(Refit.ApiException apiEx)
            {
                ElisaRequestList = null;
                await PageDialogService.DisplayAlertAsync("Error", apiEx.Message, "OK");
            }
            catch (Exception ex)
            {
                //IsRequestListVisible = false;
                ElisaRequestList = null;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand RequestSelectedCommand { get; }
        private async void OnRequestSelectedCommand()
        {
            try
            {
                if (SelectedRequestLookup != null)
                {
                    if (!SelectedRequestList.Any(x => x.request_id == SelectedRequestLookup.request_id))
                    {
                        SelectedRequestList.Add(SelectedRequestLookup);
                    }
                    IsRequestListVisible = false;
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand SetDistributionCommand { get; }
        private async void OnSetDistributionCommand()
        {
            try
            {
                IsBusy = true;

                if (!SelectedRequestList.Any())
                    throw new Exception("Select at least one request");

                var requestIds = _selectedRequestList.Select(x => x.request_id).ToArray();
                _ = await _requestService.EnableElisaRequestProcessAssaysByRequest(requestIds);

                //Create elisa multirequest event
                var elisaEvent = new ElisaEvent()
                {
                    agents = _selectedPathogenIds.ToArray(),
                    request_process = SelectedRequestList.Select(x=> new RequestProcess {
                        request_id = x.request_id,
                        process_id = x.process_id,
                        request_code = x.request_code,
                    }).ToArray()
                };
                var newEventId = await _requestService.CreateEvent(elisaEvent);

                elisaEvent = await _requestService.GetEvent(newEventId);
                if (string.IsNullOrEmpty(elisaEvent.event_code))
                {
                    elisaEvent.event_code = "";
                }
                if (elisaEvent.status_id == 0)
                {
                    elisaEvent.status_id = 41;
                    elisaEvent.status_name = "in distribution";
                }

                INavigationParameters navigationParameters = new NavigationParameters {
                    //{ "SelectedRequestList", SelectedRequestList},
                    { "SelectedPathogenList", PathogenList.Where(x => x.IsSelected).ToArray()},
                    { "ElisaEvent", elisaEvent }
                };

                var result = await NavigationService.NavigateAsync("/NavigationPage/MultiRequestsPage/MultiRequestPage", navigationParameters);
                if (!result.Success)
                {
                    throw result.Exception;
                }
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
        public DelegateCommand<object> RemoveRequestCommand { get; }
        private async void OnRemoveRequestCommand(object requestId)
        {
            try
            {
                if (SelectedRequestList == null)
                {
                    return;
                }

                RequestLookup request = SelectedRequestList.FirstOrDefault(x => x.request_id == (int)requestId);
                if(request != null)
                {
                    _ = SelectedRequestList.Remove(request);
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
                var apiResponse = await _requestService.GetElisaAgents();

                if (apiResponse.Status == 200)
                {
                    var tempPathogenItemList = new List<PathogenItem>();
                    foreach (var agent in apiResponse.Data)
                    {
                        tempPathogenItemList.Add(new PathogenItem()
                        {
                            PathogenId = agent.agent_id,
                            PathogenName = agent.agent_name
                        });
                    }
                    PathogenList = tempPathogenItemList;
                }
                else
                    throw new Exception(apiResponse.Message);

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

