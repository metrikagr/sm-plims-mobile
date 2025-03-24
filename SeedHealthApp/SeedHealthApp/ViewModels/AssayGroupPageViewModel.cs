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
using System.Linq;
using System.Threading.Tasks;

namespace SeedHealthApp.ViewModels
{
    public class AssayGroupPageViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;
        private readonly IDialogService _dialogService;


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
        private IEnumerable<SampleType> _sampleList;
        public IEnumerable<SampleType> SampleList
        {
            get { return _sampleList; }
            set { SetProperty(ref _sampleList, value); }
        }
        private IEnumerable<SampleType> _activeSampleList;
        public IEnumerable<SampleType> ActiveSampleList
        {
            get { return _activeSampleList; }
            set { SetProperty(ref _activeSampleList, value); }
        }
        private IEnumerable<int> _repetitionList;
        public IEnumerable<int> RepetitionList
        {
            get { return _repetitionList; }
            set { SetProperty(ref _repetitionList, value); }
        }
        private IEnumerable<ContainerType> _typeList;
        public IEnumerable<ContainerType> TypeList
        {
            get { return _typeList; }
            set { SetProperty(ref _typeList, value); }
        }
        private bool _isRepetitionMaxVisible;
        public bool IsRepetitionMaxVisible
        {
            get { return _isRepetitionMaxVisible; }
            set { SetProperty(ref _isRepetitionMaxVisible, value); }
        }
        private bool _isTypeVisible;
        public bool IsTypeVisible
        {
            get { return _isTypeVisible; }
            set { SetProperty(ref _isTypeVisible, value); }
        }
        private SampleType _SelectedSampleType;
        public SampleType SelectedSampleType
        {
            get { return _SelectedSampleType; }
            set { SetProperty(ref _SelectedSampleType, value); }
        }
        private string _startDate;
        public string StartDate
        {
            get { return _startDate; }
            set { SetProperty(ref _startDate, value); }
        }
        private int? _selectedActivity;
        public int? SelectedActivity
        {
            get { return _selectedActivity; }
            set { SetProperty(ref _selectedActivity, value); }
        }
        private ContainerType _selectedType;
        public ContainerType SelectedType
        {
            get { return _selectedType; }
            set { SetProperty(ref _selectedType, value); }
        }
        private string _goToSampleSettingsButtonText;
        public string GoToSampleSettingsButtonText
        {
            get { return _goToSampleSettingsButtonText; }
            set { SetProperty(ref _goToSampleSettingsButtonText, value); }
        }
        private bool _isSamplePending;
        public bool IsSamplePending
        {
            get { return _isSamplePending; }
            set { SetProperty(ref _isSamplePending, value); }
        }
        private AssaySample _selectedAssaySample;
        public AssaySample SelectedAssaySample
        {
            get { return _selectedAssaySample; }
            set { SetProperty(ref _selectedAssaySample, value); }
        }
        private SampleType _selectedActiveAssaySampleType;
        public SampleType SelectedActiveAssaySampleType
        {
            get { return _selectedActiveAssaySampleType; }
            set { SetProperty(ref _selectedActiveAssaySampleType, value); }
        }
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set { SetProperty(ref _isRefreshing, value); }
        }
        private bool _isListEmpty;
        public bool IsListEmpty
        {
            get { return _isListEmpty; }
            set { SetProperty(ref _isListEmpty, value); }
        }
        private IEnumerable<SampleType> _reprocessActiveSampleList;
        public IEnumerable<SampleType> ReprocessActiveSampleList
        {
            get { return _reprocessActiveSampleList; }
            set { SetProperty(ref _reprocessActiveSampleList, value); }
        }
        private SampleType _selectedReprocessActiveAssaySampleType;
        public SampleType SelectedReprocessActiveAssaySampleType
        {
            get { return _selectedReprocessActiveAssaySampleType; }
            set { SetProperty(ref _selectedReprocessActiveAssaySampleType, value); }
        }
        private bool _isReprocessListEmpty;
        public bool IsReprocessListEmpty
        {
            get { return _isReprocessListEmpty; }
            set { SetProperty(ref _isReprocessListEmpty, value); }
        }
        public AssayGroupPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IRequestService requestService, IDialogService dialogService, IEventAggregator eventAggregator)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _requestService = requestService;
            _dialogService = dialogService;

            NavigateToRequestAssaySampleTypeCommand = new DelegateCommand(OnNavigateToRequestAssaySampleTypeCommand, () => { return !IsBusy; });

            AddRequestProcessAssaySampleTypeCommmand = new DelegateCommand(OnAddRequestProcessAssaySampleTypeCommmand, () => { return !IsBusy; });

            Title = "Assay Manage";
            Username = SettingsService.Username;
            GoToSampleSettingsButtonText = "Start";

            NavigationUriList.Add(new BreadcumItem
            {
                Order = 1,
                Title = "Requests",
                NavigationUri = nameof(RequestsPageViewModel),
                IsFirst = true
            });
        }

        public DelegateCommand AddRequestProcessAssaySampleTypeCommmand { get;}
        private async void OnAddRequestProcessAssaySampleTypeCommmand()
        {
            try
            {
                IsBusy = true;

                IDialogParameters navigationParemeters = new DialogParameters
                {
                    { "InactiveSampleTypeList", SampleList.Where(x => !x.is_active).ToList()},
                    { "RepetitionList", RepetitionList},
                    { "ContainerTypeList", TypeList},
                    { "IsRepetitionCountVisible", IsRepetitionMaxVisible},
                    { "IsContainerTypeVisible", IsTypeVisible}
                };

                var result = await _dialogService.ShowDialogAsync("AddRequestProcessAssaySampleTypeDialog", navigationParemeters);
                if (result.Parameters.ContainsKey("SelectedSampleType"))
                {
                    var tempSelectedSampleType =  result.Parameters.GetValue<SampleType>("SelectedSampleType");
                    var tempSelectedActivityCount = result.Parameters.GetValue<int?>("SelectedActivityCount");
                    var tempSelectedContainerType = result.Parameters.GetValue<ContainerType>("SelectedContainerType");

                    var newAssaySample = new AssaySample
                    {
                        is_active = tempSelectedSampleType.is_active,
                        status_id = 42, //In process
                    };

                    if (IsRepetitionMaxVisible)
                        newAssaySample.activity_qty = tempSelectedActivityCount;
                    if (IsTypeVisible)
                        newAssaySample.type_id = tempSelectedContainerType.type_id;

                    var apiResponse = await _requestService.UpdateRequestProcessAssay(tempSelectedSampleType.request_process_essay_id, newAssaySample);
                    if (apiResponse.Status == 200)
                    {
                        await LoadAssays();
                    }
                    else
                    {
                        throw new Exception(apiResponse.Message);
                    }
                }

                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }

        public DelegateCommand NavigateToRequestAssaySampleTypeCommand { get; }
        private async void OnNavigateToRequestAssaySampleTypeCommand()
        {
            try
            {
                IsBusy = true;

                if (SelectedActiveAssaySampleType == null)
                    throw new Exception("Assay sample type is empty");

                //if (SelectedActiveAssaySampleType.status_id == 43) //Finished
                //{
                //    throw new Exception("Sample evaluation is finished");
                //}

                string NavigationUri = string.Empty;
                switch (Essay.assay_id)
                {
                    case (int)AssayMobileEnum.BlotterAndFreezePaperTest:
                    case (int)AssayMobileEnum.GerminationTest:
                        NavigationUri = "AssayPage";
                        break;
                    case (int)AssayMobileEnum.Elisa:
                        NavigationUri = "ResultsElisaPage";
                        break;
                    case (int)AssayMobileEnum.Pcr:
                        NavigationUri = "AssayPage"; //"ResultsPcrPage";
                        break;
                    default:
                        throw new Exception("Assay not supported");
                }

                INavigationParameters navigationParemeters = new NavigationParameters
                {
                    { "Request", Request},
                    { "Assay", Essay},
                    { "SelectedActiveAssaySampleType", SelectedActiveAssaySampleType}
                };

                var result = await NavigationService.NavigateAsync(NavigationUri, navigationParemeters);

                IsBusy = false;
            }
            catch(Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
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

                await LoadAssays();

                IsRefreshing = false;
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
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                IsBusy = true;
                
                if (parameters.ContainsKey("Request"))
                {
                    Request = (Request)parameters["Request"];
                    NavigationUriList.Add(new BreadcumItem
                    {
                        Order = 2,
                        Title = $"Manage Request ({_request.request_code})",
                        NavigationUri = nameof(RequestsPageViewModel)
                    });
                }
                if (parameters.ContainsKey("Essay"))
                {
                    Essay = (Assay)parameters["Essay"];
                    Title = $"Assay Manage ({Essay.assay_name})";
                    NavigationUriList.Add(new BreadcumItem
                    {
                        Order = 3,
                        Title = Title,
                        NavigationUri = nameof(AssayGroupPageViewModel)
                    });

                    switch (Essay.assay_id)
                    {
                        case (int)AssayMobileEnum.BlotterAndFreezePaperTest:
                            IsRepetitionMaxVisible = true;
                            break;
                        case (int)AssayMobileEnum.GerminationTest:
                            IsTypeVisible = true;
                            IsRepetitionMaxVisible = true;
                            break;
                    }
                    
                    RepetitionList = new List<int> {1,2,3,4,5,6,7,8,9,10};
                    
                    TypeList = new List<ContainerType>{
                        new ContainerType{type_id = 50, type_name = "Paper"},
                        new ContainerType{ type_id = 56, type_name = "Soil"}
                    };
                }
                
                if(ActiveSampleList == null)
                {
                    await LoadAssays();
                }

                if (parameters.ContainsKey("refresh"))
                {
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
            var tempSampleList = await _requestService.GetRequestProcessAssays(Request.request_id, 0, Essay.assay_id);
            SampleList = tempSampleList.Where(x => x.process_id == 1);
            ActiveSampleList = tempSampleList.Where(x => x.is_active && x.process_id == 1).ToList();
            IsListEmpty = !ActiveSampleList.Any();

            ReprocessActiveSampleList = tempSampleList.Where(x => x.process_id > 1).ToArray();
            IsReprocessListEmpty = !ReprocessActiveSampleList.Any();
        }
    }
}
