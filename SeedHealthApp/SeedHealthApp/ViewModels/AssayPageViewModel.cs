using Microcharts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SeedHealthApp.Models;
using SeedHealthApp.Models.Db;
using SeedHealthApp.Models.Dtos;
using SeedHealthApp.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SeedHealthApp.Custom.Events;
using System.Diagnostics;
using SeedHealthApp.Extensions;

namespace SeedHealthApp.ViewModels
{
    public class AssayPageViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;
        private Request _request;
        private Assay _assay;
        private SampleType _selectedActiveAssaySampleType;
        private bool _isActivated;
        private bool _ignoreOnce;
        private bool _refreshIsNeeded;
        public SampleType SelectedActiveAssaySampleType
        {
            get { return _selectedActiveAssaySampleType; }
            set { SetProperty(ref _selectedActiveAssaySampleType, value); }
        }
        private RequestProcessAssaySteps _requestProcessAssayStepList;
        public RequestProcessAssaySteps RequestProcessAssayStepList
        {
            get { return _requestProcessAssayStepList; }
            set { SetProperty(ref _requestProcessAssayStepList, value); }
        }
        private DonutChart _step1DonutChart;
        public DonutChart Step1DonutChart
        {
            get => _step1DonutChart;
            set => SetProperty(ref _step1DonutChart, value);
        }
        private DonutChart _step2DonutChart;
        public DonutChart Step2DonutChart //RadialGaugeChart
        {
            get => _step2DonutChart;
            set => SetProperty(ref _step2DonutChart, value);
        }
        private DonutChart _step3DonutChart;
        public DonutChart Step3DonutChart
        {
            get { return _step3DonutChart; }
            set { SetProperty(ref _step3DonutChart, value); }
        }
        private bool _isStep1Pending;
        public bool IsAssayPending
        {
            get { return _isStep1Pending; }
            set { SetProperty(ref _isStep1Pending, value); }
        }
        private string _step1Status = string.Empty;
        public string Step1Status
        {
            get { return _step1Status; }
            set { SetProperty(ref _step1Status, value); }
        }
        private string _step2Status = string.Empty;
        public string Step2Status
        {
            get { return _step2Status; }
            set { SetProperty(ref _step2Status, value); }
        }
        private bool _hasLocalChanges;
        public bool HasLocalChanges
        {
            get { return _hasLocalChanges; }
            set { SetProperty(ref _hasLocalChanges, value); }
        }
        private bool _hasSteps;
        public bool HasSteps
        {
            get { return _hasSteps; }
            set { SetProperty(ref _hasSteps, value); }
        }
        private bool _isGerminationTest;
        public bool IsGerminationTest
        {
            get { return _isGerminationTest; }
            set { SetProperty(ref _isGerminationTest, value); }
        }
        private string _subProcess1Name;
        public string SubProcess1Name
        {
            get { return _subProcess1Name; }
            set { SetProperty(ref _subProcess1Name, value); }
        }
        private string _subProcess2Name;
        public string SubProcess2Name
        {
            get { return _subProcess2Name; }
            set { SetProperty(ref _subProcess2Name, value); }
        }
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set { SetProperty(ref _isRefreshing, value); }
        }
        public bool ModifiedOnly { get; set; }
        public AssayPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IRequestService requestService, IEventAggregator eventAggregator)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _requestService = requestService;

            Username = SettingsService.Username;

            NavigateToEvaluationStep1Command = new DelegateCommand(OnNavigateToEvaluationStep1Command, () => { return IsIdle; });
            NavigateToEvaluationStep2Command = new DelegateCommand(OnNavigateToEvaluationStep2Command, () => { return IsIdle; });
            NavigateToEvaluationStep3Command = new DelegateCommand(OnNavigateToEvaluationStep3Command, () => { return IsIdle; });
            FinishStep1Command = new DelegateCommand(OnFinishStep1Command);
            FinishStep2Command = new DelegateCommand(OnFinishStep2Command);

            SetConnectivityModeCommand = new DelegateCommand(OnSetConnectivityModeCommand);
            SyncCommand = new DelegateCommand(ExecuteSyncCommand);
            ShowDebugInfoCommand = new DelegateCommand(OnShowDebugInfoCommand);

            FinishCommand = new DelegateCommand(ExecFinishCommand);

            HasLocalChanges = false;
            _ = EventAggregator.GetEvent<RequestProcessAssayActivitySampleUpdatedEvent>().Subscribe(RefreshIsNeeded);
            _ = EventAggregator.GetEvent<ResultUpdatedEvent>().Subscribe(RefreshIsNeeded);

            NavigationUriList.Add(new BreadcumItem
            {
                Order = 1,
                Title = "Requests",
                NavigationUri = nameof(RequestsPageViewModel),
                IsFirst = true
            });
        }
        private void RefreshIsNeeded()
        {
            if (!_refreshIsNeeded)
                _refreshIsNeeded = true;
        }
        public DelegateCommand ShowDebugInfoCommand { get; }
        private async void OnShowDebugInfoCommand()
        {
            try
            {
                string message = Newtonsoft.Json.JsonConvert.SerializeObject(_request, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Newtonsoft.Json.JsonConvert.SerializeObject(_assay, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Newtonsoft.Json.JsonConvert.SerializeObject(SelectedActiveAssaySampleType, Newtonsoft.Json.Formatting.Indented);
                await PageDialogService.DisplayAlertAsync("", message , "OK");
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        private DelegateCommand _showLocalActivitySampleInfoCommand;
        public DelegateCommand ShowLocalActivitySampleInfoCommand =>
            _showLocalActivitySampleInfoCommand ?? (_showLocalActivitySampleInfoCommand = new DelegateCommand(ExecuteShowLocalActivitySampleInfoCommand));

        private async void ExecuteShowLocalActivitySampleInfoCommand()
        {
            try
            {
                IsBusy = true;
                string message = string.Empty;
                if (ModifiedOnly)
                {
                    var localAssayActivitySamples = await _requestService.GetAssayActivitySampleChangesLocal(SelectedActiveAssaySampleType.request_process_essay_id);
                    message = Newtonsoft.Json.JsonConvert.SerializeObject(localAssayActivitySamples, Newtonsoft.Json.Formatting.Indented);
                }
                else
                {
                    var localAssayActivitySamples = await _requestService.GetAssayActivitySamples(SelectedActiveAssaySampleType.request_process_essay_id, true, null, null, null, null);
                    message = Newtonsoft.Json.JsonConvert.SerializeObject(localAssayActivitySamples, Newtonsoft.Json.Formatting.Indented);
                }
                await PageDialogService.DisplayAlertAsync("", message, "OK");
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayErrorAlertAsync(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private DelegateCommand _showLocalDBInfoCommand;
        public DelegateCommand ShowLocalDBInfoCommand =>
            _showLocalDBInfoCommand ?? (_showLocalDBInfoCommand = new DelegateCommand(ExecuteShowLocalDBInfoCommand));

        private async void ExecuteShowLocalDBInfoCommand()
        {
            try
            {
                IsBusy = true;
                string message = string.Empty;
                if (ModifiedOnly)
                {
                    var localResults = await _requestService.GetResultChangesLocal(SelectedActiveAssaySampleType.request_process_essay_id);
                    message = Newtonsoft.Json.JsonConvert.SerializeObject(localResults, Newtonsoft.Json.Formatting.Indented);
                }
                else
                {
                    var localResults = await _requestService.GetResults(SelectedActiveAssaySampleType.request_process_essay_id, null, null, true);
                    message = Newtonsoft.Json.JsonConvert.SerializeObject(localResults, Newtonsoft.Json.Formatting.Indented);
                }

                await PageDialogService.DisplayAlertAsync("", message, "OK");
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayErrorAlertAsync(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public DelegateCommand NavigateToEvaluationStep1Command { get; }
        private async void OnNavigateToEvaluationStep1Command()
        {
            try
            {
                IsBusy = true;

                if (SelectedActiveAssaySampleType == null)
                    throw new Exception("Assay is empty");
                if (IsOffline && !SelectedActiveAssaySampleType.IsAvailableOffline)
                    throw new Exception("Current assay is not available offline");

                bool checkStatus = false;

                string NavigationUri = string.Empty;
                switch (_assay.assay_id)
                {
                    case (int)AssayMobileEnum.BlotterAndFreezePaperTest:
                    case (int)AssayMobileEnum.GerminationTest:
                        NavigationUri = "AssayPreparationPage";
                        checkStatus = true;
                        break;
                    case (int)AssayMobileEnum.Elisa:
                        NavigationUri = "ResultsElisaPage";
                        break;
                    case (int)AssayMobileEnum.Pcr:
                        NavigationUri = "ResultsPcrPage";
                        break;
                    default:
                        throw new Exception("Assay not supported");
                }

                if (checkStatus && IsAssayPending)  //Active request_process_activity_samples
                {
                    var apiResponse = await _requestService.UpdateAssayStep(SelectedActiveAssaySampleType.request_process_essay_id, 0, "init");
                    if(apiResponse.Status != 200)
                    {
                        throw new Exception(apiResponse.Message);
                    }
                    
                    //Refresh Page
                    await LoadPageData();
                }

                INavigationParameters navigationParemeters = new NavigationParameters
                {
                    { "Request", _request},
                    { "Assay", _assay},
                    { "SelectedActiveAssaySampleType", SelectedActiveAssaySampleType}
                };

                var result = await NavigationService.NavigateAsync(NavigationUri, navigationParemeters);

                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand FinishStep1Command { get; }
        private async void OnFinishStep1Command()
        {
            try
            {
                IsBusy = true;

                if (IsOffline)
                    throw new Exception("Internet is required for this action");

                var result = await PageDialogService.DisplayAlertAsync("Warning", "Are you sure to finish this assay preparation?" + Environment.NewLine
                   + "You will not be able to modify or enter new tested seeds counts", "Yes", "No");
                if (!result)
                    return;

                if (SelectedActiveAssaySampleType == null)
                    throw new Exception("Assay is empty");
                if (IsOffline && !SelectedActiveAssaySampleType.IsAvailableOffline)
                    throw new Exception("Current assay is not available offline");

                var apiResponse = await _requestService.UpdateAssayStep(SelectedActiveAssaySampleType.request_process_essay_id, 0, "finish");
                if (apiResponse.Status != 200)
                {
                    throw new Exception(apiResponse.Message);
                }
                
                //Refresh Page
                await LoadPageData();

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
        public DelegateCommand NavigateToEvaluationStep2Command { get; }
        private async void OnNavigateToEvaluationStep2Command()
        {
            try
            {
                IsBusy = true;

                if (SelectedActiveAssaySampleType == null)
                    throw new Exception("Assay sample type is empty");
                if (IsOffline && !SelectedActiveAssaySampleType.IsAvailableOffline)
                    throw new Exception("Current assay is not available offline");

                string NavigationUri = string.Empty;
                switch (_assay.assay_id)
                {
                    case (int)AssayMobileEnum.BlotterAndFreezePaperTest:
                        NavigationUri = "ResultsBlotterPage";
                        break;
                    case (int)AssayMobileEnum.GerminationTest:
                        NavigationUri = "ResultsGerminationTestPage";
                        break;
                    case (int)AssayMobileEnum.Elisa:
                        NavigationUri = "ResultsElisaPage";
                        break;
                    case (int)AssayMobileEnum.Pcr:
                        NavigationUri = "ResultsPcrPage";
                        break;
                    default:
                        throw new Exception("Assay not supported");
                }

                INavigationParameters navigationParemeters = new NavigationParameters
                {
                    { "Request", _request},
                    { "Assay", _assay},
                    { "SelectedActiveAssaySampleType", SelectedActiveAssaySampleType},
                    { "SubProcessNumber", 2 }
                };

                var result = await NavigationService.NavigateAsync(NavigationUri, navigationParemeters);

                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand NavigateToEvaluationStep3Command { get; }
        private async void OnNavigateToEvaluationStep3Command()
        {
            try
            {
                IsBusy = true;

                INavigationParameters navigationParemeters = new NavigationParameters
                {
                    { "Request", _request},
                    { "Assay", _assay},
                    { "SelectedActiveAssaySampleType", SelectedActiveAssaySampleType},
                    { "SubProcessNumber", 3 }
                };
                var result = await NavigationService.NavigateAsync("ResultsGerminationTestPage", navigationParemeters);
                if (!result.Success)
                    throw result.Exception;
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayErrorAlertAsync(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
        public DelegateCommand FinishStep2Command { get; }
        private async void OnFinishStep2Command()
        {
            try
            {
                IsBusy = true;

                if (IsOffline)
                    throw new Exception("Internet is required for this action");
                if (HasLocalChanges)
                    throw new Exception("Synchronization is required");

                var result = await PageDialogService.DisplayAlertAsync("Warning", "Are you sure to finish this assay evaluation?" + Environment.NewLine
                   + "You will not be able to modify or enter new results", "Yes", "No");
                if (!result)
                    return;

                if (SelectedActiveAssaySampleType == null)
                    throw new Exception("Assay is empty");
                if (IsOffline && !SelectedActiveAssaySampleType.IsAvailableOffline)
                    throw new Exception("Current assay is not available offline");

                var apiResponse = await _requestService.UpdateAssayStep(SelectedActiveAssaySampleType.request_process_essay_id, 1, "finish");
                if (apiResponse.Status != 200)
                {
                    throw new Exception(apiResponse.Message);
                }
                await PageDialogService.DisplayAlertAsync("", "Assay was sucessfully finished", "OK");
                
                INavigationParameters parameters = new NavigationParameters()
                {
                    { "refresh" , true}
                };
                await NavigationService.GoBackAsync(parameters);

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
        public DelegateCommand SetConnectivityModeCommand { get; }
        private async void OnSetConnectivityModeCommand()
        {
            try
            {
                if (!_isActivated)
                    return;
                if (_ignoreOnce)
                {
                    _ignoreOnce = false;
                    return;
                }
                IsBusy = true;
                
                if (IsOffline)
                    throw new Exception("Internet is required");

                SelectedActiveAssaySampleType.IsAvailableOffline = !SelectedActiveAssaySampleType.IsAvailableOffline;

                if (!SelectedActiveAssaySampleType.IsAvailableOffline && HasLocalChanges) //if it will be false but there are changes
                {
                    var result = await PageDialogService.DisplayAlertAsync("Warning", "There are some pending local changes" + Environment.NewLine
                   + "Are you sure to continue?", "Yes", "No");
                    if (!result)
                    {
                        _ignoreOnce = true;
                        SelectedActiveAssaySampleType.IsAvailableOffline = true;
                        return;
                    }
                }

                if ( new int[] { (int)AssayMobileEnum.BlotterAndFreezePaperTest, (int)AssayMobileEnum.GerminationTest}.Contains(_assay.assay_id)
                    && IsAssayPending)  //Active request_process_activity_samples
                {
                    var apiResponse = await _requestService.UpdateAssayStep(SelectedActiveAssaySampleType.request_process_essay_id, 0, "init");
                    if (apiResponse.Status != 200)
                    {
                        throw new Exception(apiResponse.Message);
                    }

                    //Refresh Page
                    var requestProcessAssay = await _requestService.GetRequestProcessAssay(SelectedActiveAssaySampleType.request_process_essay_id);
                    if (requestProcessAssay.IsAvailableOffline != SelectedActiveAssaySampleType.IsAvailableOffline)
                        requestProcessAssay.IsAvailableOffline = SelectedActiveAssaySampleType.IsAvailableOffline;
                    SelectedActiveAssaySampleType = requestProcessAssay;
                    await LoadAssaySteps();
                }

                await _requestService.CreateOrUpdateAssayLocal(SelectedActiveAssaySampleType);

                if (SelectedActiveAssaySampleType.IsAvailableOffline)
                {
                    Stopwatch stopWatch = new Stopwatch();
                    
                    //Download CompositeSample
                    await _requestService.GetRequestProcessAssaySamples(SelectedActiveAssaySampleType.request_process_essay_id);

                    if (HasSteps)
                    {
                        stopWatch.Start();
                        //Download RequestProcessAssaySample
                        var tempAssayActivitySamples = await _requestService.GetAssayActivitySamples(SelectedActiveAssaySampleType.request_process_essay_id, false);
                        stopWatch.Stop();
                        foreach (var item in tempAssayActivitySamples)
                        {
                            item.request_process_essay_id = SelectedActiveAssaySampleType.request_process_essay_id;
                        }
                        TimeSpan ts = stopWatch.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        await PageDialogService.DisplayAlertAsync("ActivitySamples downloaded", elapsedTime, "OK");
                        stopWatch.Start();
                        await _requestService.CreateManyForcedAssayActivitySampleLocal(tempAssayActivitySamples);
                        stopWatch.Stop();
                        ts = stopWatch.Elapsed;
                        elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                        await PageDialogService.DisplayAlertAsync("ActivitySamples saved locally", elapsedTime, "OK");
                        //foreach (var item in tempAssayActivitySamples)
                        //{
                        //    item.request_process_essay_id = SelectedActiveAssaySampleType.request_process_essay_id;
                        //    await _requestService.CreateAssayActivitySample(true, item);
                        //}
                    }
                    //Download Agents
                    await _requestService.GetAgents(_request.request_id, SelectedActiveAssaySampleType.process_id, _assay.assay_id, SelectedActiveAssaySampleType.sample_type_id);
                    //Download Results
                    var tempResults = await _requestService.GetResults(SelectedActiveAssaySampleType.request_process_essay_id, null, null, false);
                    foreach (var item in tempResults)
                    {
                        item.request_process_assay_id = SelectedActiveAssaySampleType.request_process_essay_id;
                        await _requestService.CreateOrUpdateResultLocal(item);
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
        private DelegateCommand _refreshCommand;
        public DelegateCommand RefreshCommand =>
            _refreshCommand ?? (_refreshCommand = new DelegateCommand(ExecuteRefreshCommand));
        private async void ExecuteRefreshCommand()
        {
            try
            {
                IsBusy = true;
                IsRefreshing = true;

                await LoadPageData();

                IsRefreshing = false;
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsRefreshing = false;
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        private async Task LoadPageData()
        {
            //Load request process assay
            SelectedActiveAssaySampleType = await _requestService.GetRequestProcessAssay(SelectedActiveAssaySampleType.request_process_essay_id);
            await LoadAssaySteps();
        }
        public DelegateCommand SyncCommand { get; }
        private async void ExecuteSyncCommand()
        {
            try
            {
                IsBusy = true;

                if (IsOffline)
                    throw new Exception("Internet is required");
                if (!SelectedActiveAssaySampleType.IsAvailableOffline)
                    throw new Exception("Current assay is not available offline");
                if (!HasLocalChanges)
                    throw new Exception("No pending changes");

                DateTime syncedDate = await _requestService.GetServerDatetime();

                var tempAssayActivitySamples = await _requestService.GetAssayActivitySampleChangesLocal(SelectedActiveAssaySampleType.request_process_essay_id);
                if (tempAssayActivitySamples.Any())
                {
                    //string message = Newtonsoft.Json.JsonConvert.SerializeObject(tempAssayActivitySamples, Newtonsoft.Json.Formatting.Indented);
                    //await PageDialogService.DisplayAlertAsync("", message, "OK");

                    var modifiedCells = tempAssayActivitySamples
                        .Select(x => new RequestProcessAssayActivitySampleBatch
                        {
                            composite_sample_id = x.composite_sample_id,
                            activity_id = x.activity_id,
                            number_of_seeds = x.number_of_seeds
                        })
                        .ToList();

                    var block = new ActionBlock<RequestProcessAssayActivitySampleBatch>(async (reqProAssayActivitySample) =>
                    {
                        await _requestService.UpdateAssayActivitySample(SelectedActiveAssaySampleType.request_process_essay_id, reqProAssayActivitySample, false);
                    }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 10 });

                    foreach (var item in modifiedCells)
                    {
                        block.Post(item); // Post all items to the block
                    }

                    block.Complete(); // Signal completion
                    await block.Completion; // Asynchronously wait for completion.

                    //Update local changes
                    _ = await _requestService.MarkAsDoneAssayActivitySampleChangesLocal(SelectedActiveAssaySampleType.request_process_essay_id);
                }
                var tempResults = await _requestService.GetResultChangesLocal(SelectedActiveAssaySampleType.request_process_essay_id);
                if (tempResults.Any())
                {
                    //await PageDialogService.DisplayAlertAsync("", Newtonsoft.Json.JsonConvert.SerializeObject(tempResults, Newtonsoft.Json.Formatting.Indented), "OK");
                    
                    var block = new ActionBlock<Result>(async (result) =>
                    {
                        await _requestService.CreateResult(SelectedActiveAssaySampleType.request_process_essay_id, result, false);
                    }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 10 });

                    foreach (var item in tempResults)
                    {
                        block.Post(item); // Post all items to the block
                    }

                    block.Complete(); // Signal completion
                    await block.Completion; // Asynchronously wait for completion.
                    
                    //Update local changes
                    _ = await _requestService.MarkAsDoneResultChangesLocal(SelectedActiveAssaySampleType.request_process_essay_id);
                }

                SelectedActiveAssaySampleType.LastSyncedDate = syncedDate;
                await _requestService.CreateOrUpdateAssayLocal(SelectedActiveAssaySampleType);

                HasLocalChanges = await _requestService.HasChangesLocal(SelectedActiveAssaySampleType.request_process_essay_id);

                // TODO: Active after fix GetSteps
                //await LoadPageData();
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

        public DelegateCommand FinishCommand { get; }
        private async void ExecFinishCommand()
        {
            try
            {
                IsBusy = true;

                if (IsOffline)
                    throw new Exception("Internet is required for this action");
                
                bool resultIsOk = await PageDialogService.DisplayAlertAsync("Warning",
                    "Are you sure to finish this evaluation?" + Environment.NewLine + "You will not be able to modify or enter new results",
                    "Yes", "No");
                if (resultIsOk)
                {
                    _ = await _requestService.UpdateRequestProcessAssayStatus(SelectedActiveAssaySampleType.request_process_essay_id, (int)StatusEnum.Finished);

                    await PageDialogService.DisplayAlertAsync("", "Assay was sucessfully finished", "OK");
                    _ = await NavigationService.GoBackAsync(new NavigationParameters()
                    {
                        { "refresh" , true}
                    });
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
        public override void Initialize(INavigationParameters parameters)
        {
            try
            {
                IsBusy = true;

                if (parameters.ContainsKey("Request"))
                {
                    _request = parameters.GetValue<Request>("Request");
                    NavigationUriList.Add(new BreadcumItem
                    {
                        Order = 2,
                        Title = $"Manage Request ({_request.request_code})",
                        NavigationUri = nameof(RequestsPageViewModel)
                    });
                }
                if (parameters.ContainsKey("Assay"))
                {
                    _assay = parameters.GetValue<Assay>("Assay");
                    NavigationUriList.Add(new BreadcumItem
                    {
                        Order = 3,
                        Title = $"Assay Manage ({_assay.assay_name})",
                        NavigationUri = nameof(AssayGroupPageViewModel)
                    });

                    switch (_assay.assay_id)
                    {
                        case (int)AssayMobileEnum.BlotterAndFreezePaperTest:
                            SubProcess1Name = "Sowing";
                            SubProcess2Name = "Evaluation";
                            HasSteps = true;
                            break;
                        case (int)AssayMobileEnum.GerminationTest:
                            HasSteps = true;
                            SubProcess1Name = "Sowing";
                            SubProcess2Name = "Germination evaluation";
                            IsGerminationTest = true;
                            break;
                        default:
                            HasSteps = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
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

                if (parameters.ContainsKey("SelectedActiveAssaySampleType"))
                {
                    var tempSelectedActiveAssaySampleType = parameters.GetValue<SampleType>("SelectedActiveAssaySampleType");
                    Title = $"Sample Results Management ({tempSelectedActiveAssaySampleType.sample_type_name})";
                    NavigationUriList.Add(new BreadcumItem
                    {
                        Order = 4,
                        Title = Title,
                        NavigationUri = nameof(AssayPageViewModel)
                    });

                    if (tempSelectedActiveAssaySampleType.activity_list == null || !tempSelectedActiveAssaySampleType.activity_list.Any())
                        throw new Exception("Activity list is empty");

                    if (tempSelectedActiveAssaySampleType.status_name.Equals("pending"))
                    {
                        //await PageDialogService.DisplayAlertAsync("Starting assay", $"Assay status {tempSelectedActiveAssaySampleType.status_name}", "OK");

                        var apiResponse = await _requestService.UpdateAssayStep(tempSelectedActiveAssaySampleType.request_process_essay_id, 0, "init");
                        if (apiResponse.Status != 200)
                        {
                            throw new Exception(apiResponse.Message);
                        }
                    }

                    //Get ReqProAssay to get IsAvailableOffline and LastSyncedDate
                    tempSelectedActiveAssaySampleType = await _requestService.GetRequestProcessAssay(tempSelectedActiveAssaySampleType.request_process_essay_id);
                    //var localAssay = await _requestService.GetRequestProcessAssay(tempSelectedActiveAssaySampleType.request_process_essay_id);
                    //if (localAssay != null)
                    //    tempSelectedActiveAssaySampleType.IsAvailableOffline = localAssay.IsAvailableOffline;

                    if (tempSelectedActiveAssaySampleType.assay_id == 0)
                        tempSelectedActiveAssaySampleType.assay_id = _assay.assay_id;

                    //Set offline mode by default
                    if (!tempSelectedActiveAssaySampleType.IsAvailableOffline)
                    {
                        //Activating offline mode
                        //await PageDialogService.DisplayAlertAsync("Activating offline mode", $"Assay status {tempSelectedActiveAssaySampleType.status_name}", "OK");

                        if (IsOffline)
                            throw new Exception("Internet is required");

                        //Download Samples
                        await _requestService.GetRequestProcessAssaySamples(tempSelectedActiveAssaySampleType.request_process_essay_id);
                        //Download Agents
                        await _requestService.GetAgents(_request.request_id, tempSelectedActiveAssaySampleType.process_id, _assay.assay_id, tempSelectedActiveAssaySampleType.sample_type_id);
                        //Download RequestProcessAssaySample
                        if (HasSteps)
                        {
                            Stopwatch downloadWatch = new Stopwatch();
                            downloadWatch.Start();
                            var tempAssayActivitySamples = await _requestService.GetAssayActivitySamples(tempSelectedActiveAssaySampleType.request_process_essay_id, false);
                            downloadWatch.Stop();

                            foreach (var item in tempAssayActivitySamples)
                            {
                                item.request_process_essay_id = tempSelectedActiveAssaySampleType.request_process_essay_id;
                            }
                            TimeSpan ts = downloadWatch.Elapsed;
                            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                            //await PageDialogService.DisplayAlertAsync("ActivitySamples downloaded", elapsedTime, "OK");

                            if (tempAssayActivitySamples.Any())
                            {
                                Stopwatch storingWatch = new Stopwatch();
                                storingWatch.Start();
                                await _requestService.CreateManyForcedAssayActivitySampleLocal(tempAssayActivitySamples);
                                storingWatch.Stop();
                                ts = storingWatch.Elapsed;
                                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                                //await PageDialogService.DisplayAlertAsync("ActivitySamples saved locally", elapsedTime, "OK");
                            }
                        }
                        //Download Results
                        var tempResults = await _requestService.GetResults(tempSelectedActiveAssaySampleType.request_process_essay_id, null, null, false);
                        foreach (var item in tempResults)
                        {
                            item.request_process_assay_id = tempSelectedActiveAssaySampleType.request_process_essay_id;
                            await _requestService.CreateOrUpdateResultLocal(item);
                        }

                        //Save locally request process assay
                        tempSelectedActiveAssaySampleType.IsAvailableOffline = true;
                        await _requestService.CreateOrUpdateAssayLocal(tempSelectedActiveAssaySampleType);
                    }
                    
                    SelectedActiveAssaySampleType = tempSelectedActiveAssaySampleType;
                    IsAssayPending = SelectedActiveAssaySampleType.status_name.Equals("pending");
                    if (SelectedActiveAssaySampleType.status_name.Equals("finished"))
                        Step1Status = SelectedActiveAssaySampleType.status_name;
                    // TODO: Active after fix GetSteps
                    //if (RequestProcessAssayStepList == null)
                    //{
                    //    await LoadAssaySteps();
                    //}
                }

                //Always review and show pending local changes status
                if (SelectedActiveAssaySampleType != null)
                {
                    HasLocalChanges = await _requestService.HasChangesLocal(SelectedActiveAssaySampleType.request_process_essay_id);
                }

                //In case we have saved any changes on the preparation page or the results page
                if (_refreshIsNeeded)
                {
                    if(!IsOffline && !SelectedActiveAssaySampleType.IsAvailableOffline)
                        await LoadPageData();
                    _refreshIsNeeded = false;
                }

                _isActivated = true;
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
            finally{
                IsBusy = false;
            }
        }
        private async Task LoadAssaySteps(bool forceOnline = true)
        {
            IsAssayPending = SelectedActiveAssaySampleType.status_name.Equals("pending"); //TODO: Change this after AssaySteps WebAPI has been fixed
            if (HasSteps) //TODO: Move this validation out
            {
                //await PageDialogService.DisplayAlertAsync("", "Loading steps info", "OK");

                var tempRequestProcessAssayStepList = await _requestService.GetAssaySteps(
                    SelectedActiveAssaySampleType.request_process_essay_id,
                    _assay.assay_id,
                    !forceOnline
                );

                if (string.IsNullOrEmpty(tempRequestProcessAssayStepList?.preparation?.status))
                {
                    tempRequestProcessAssayStepList.preparation.status = string.Empty;
                    Step1Status = string.Empty;
                }
                else
                    Step1Status = tempRequestProcessAssayStepList?.preparation?.status;

                if (string.IsNullOrEmpty(tempRequestProcessAssayStepList?.evaluation?.status))
                {
                    tempRequestProcessAssayStepList.evaluation.status = string.Empty;
                    Step2Status = string.Empty;
                }
                else
                    Step2Status = tempRequestProcessAssayStepList?.evaluation?.status;

                tempRequestProcessAssayStepList.preparation.percent = (float)Math.Round(tempRequestProcessAssayStepList.preparation.percent, 0);
                tempRequestProcessAssayStepList.evaluation.percent = (float)Math.Round(tempRequestProcessAssayStepList.evaluation.percent, 0);
                
                RequestProcessAssayStepList = tempRequestProcessAssayStepList;

                SKColor blueColor = SKColor.Parse("#09C");
                SKColor redColor = SKColor.Parse("#CC0000");
                SKColor backgroundColor = SKColor.Parse("#00FFFFFF");

                var preparationChartEntries = new List<ChartEntry>()
                {
                    new ChartEntry(RequestProcessAssayStepList.preparation.percent) { Color = blueColor },
                    new ChartEntry(100 - RequestProcessAssayStepList.preparation.percent) { Color = redColor },
                };
                Step1DonutChart = new DonutChart { Entries = preparationChartEntries, Margin = 4, HoleRadius = 0.6f, BackgroundColor = backgroundColor, IsAnimated = false };

                var evaluationChartEntries = new List<ChartEntry>()
                {
                    new ChartEntry(RequestProcessAssayStepList.evaluation.percent) { Color = blueColor},
                    new ChartEntry(100 - RequestProcessAssayStepList.evaluation.percent) { Color = redColor}
                };
                //Step2DonutChart = new RadialGaugeChart { Entries = evaluationChartEntries, Margin = 0, MaxValue = 100f, LineSize = 18, BackgroundColor = backgroundColor };
                Step2DonutChart = new DonutChart { Entries = evaluationChartEntries, Margin = 4, HoleRadius = 0.6f, BackgroundColor = backgroundColor, IsAnimated = false };

                if (IsGerminationTest)
                {
                    tempRequestProcessAssayStepList.sub_process_3.percent = (float)Math.Round(tempRequestProcessAssayStepList.sub_process_3.percent, 0);
                    var subProcess3ChartEntries = new List<ChartEntry>()
                    {
                        new ChartEntry(RequestProcessAssayStepList.sub_process_3.percent) { Color = blueColor},
                        new ChartEntry(100 - RequestProcessAssayStepList.sub_process_3.percent) { Color = redColor}
                    };
                    Step3DonutChart = new DonutChart { Entries = subProcess3ChartEntries, Margin = 4, HoleRadius = 0.6f, BackgroundColor = backgroundColor, IsAnimated = false };
                }
            }
        }
        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            _isActivated = false;
            base.OnNavigatedFrom(parameters);
        }
        
    }
}
