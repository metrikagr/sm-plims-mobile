using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Prism.Services.Dialogs;
using SeedHealthApp.Custom.Events;
using SeedHealthApp.Models;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SeedHealthApp.ViewModels
{
    public class ResultsBlotterPageViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;
        private readonly IDialogService _dialogService;
        private readonly IToastService _toastService;
        private IEnumerable<SampleMaterial> MaterialList;

        private SampleType _SelectedSampleType;
        public SampleType SelectedSampleType
        {
            get { return _SelectedSampleType; }
            set { SetProperty(ref _SelectedSampleType, value); }
        }
        
        private IEnumerable<Activity> _activityList;
        public IEnumerable<Activity> ActivityList
        {
            get { return _activityList; }
            set { SetProperty(ref _activityList, value); }
        }
        private Activity _selectedActivity;
        public Activity SelectedActivity
        {
            get { return _selectedActivity; }
            set { SetProperty(ref _selectedActivity, value); }
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
        private AssaySample _selectedAssaySample;
        public AssaySample SelectedAssaySample
        {
            get { return _selectedAssaySample; }
            set { SetProperty(ref _selectedAssaySample, value); }
        }

        private ObservableCollection<PathogenItem> _selectedPathogenItemList;
        public ObservableCollection<PathogenItem> SelectedPathogenItemList
        {
            get { return _selectedPathogenItemList; }
            set { SetProperty(ref _selectedPathogenItemList, value); }
        }
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); }
        }
        private ObservableCollection<PathogenItem> _pathogenItemList;
        public ObservableCollection<PathogenItem> PathogenItemList
        {
            get { return _pathogenItemList; }
            set { SetProperty(ref _pathogenItemList, value); }
        }
        private SampleMaterial _selectedMaterial;
        public SampleMaterial SelectedMaterial
        {
            get { return _selectedMaterial; }
            set { SetProperty(ref _selectedMaterial, value); }
        }
        private ObservableCollection<ResultCell> _currentResults;
        public ObservableCollection<ResultCell> CurrentResults
        {
            get { return _currentResults; }
            set { SetProperty(ref _currentResults, value); }
        }
        private int _seedsMaxCount;
        public int SeedsMaxCount
        {
            get { return _seedsMaxCount; }
            set { SetProperty(ref _seedsMaxCount, value); }
        }
        private bool _keepPathogens;
        public bool KeepPathogens
        {
            get { return _keepPathogens; }
            set { SetProperty(ref _keepPathogens, value); }
        }
        private bool _markedToReprocess;
        public bool MarkedToReprocess
        {
            get { return _markedToReprocess; }
            set { SetProperty(ref _markedToReprocess, value); }
        }
        public ResultsBlotterPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IDialogService dialogService, IRequestService requestService, IEventAggregator eventAggregator, IToastService toastService)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _dialogService = dialogService;
            _toastService = toastService;
            _requestService = requestService;

            OpenPathogenPickerCommand = new DelegateCommand(OnOpenPathogenPickerCommand);
            PreviousMaterialCommand = new DelegateCommand(OnPreviousMaterialCommand);
            NextMaterialCommand = new DelegateCommand(OnNextMaterialCommand);
            SaveEvaluationCommmand = new DelegateCommand(OnSaveEvaluationCommmand);
            FinishEvaluationCommand = new DelegateCommand(OnFinishEvaluationCommand);

            PreviousActivityCommand = new DelegateCommand(OnPreviousActivityCommand);
            NextActivityCommand = new DelegateCommand(OnNextActivityCommand);

            ShowDebugInfoCommand = new DelegateCommand(OnShowDebugInfoCommand);
            Title = "Blotter Results";
            Username = SettingsService.Username;

        }
        public DelegateCommand ShowDebugInfoCommand { get; }
        private async void OnShowDebugInfoCommand()
        {
            try
            {
                string message = Newtonsoft.Json.JsonConvert.SerializeObject(_request, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Newtonsoft.Json.JsonConvert.SerializeObject(SelectedSampleType, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Newtonsoft.Json.JsonConvert.SerializeObject(SelectedMaterial, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Newtonsoft.Json.JsonConvert.SerializeObject(SelectedActivity, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Newtonsoft.Json.JsonConvert.SerializeObject(CurrentResults, Newtonsoft.Json.Formatting.Indented);
                await PageDialogService.DisplayAlertAsync("", message, "OK");
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        public DelegateCommand OpenPathogenPickerCommand { get; }
        private async void OnOpenPathogenPickerCommand()
        {
            try
            {
                //foreach (var item in PathogenItemList.Where(x => x.IsSelected))
                //{
                //    item.IsSelected = false;
                //}
                DialogParameters dialogParameters = new DialogParameters
                {
                    {"PathogenItemList", PathogenItemList}
                };
                var result = await _dialogService.ShowDialogAsync("PathogenPickerDialog", dialogParameters);
                if (result.Parameters.ContainsKey("SelectedPathogenItemList"))
                {
                    var selectedPathogenItems = (IEnumerable<PathogenItem>)result.Parameters["SelectedPathogenItemList"];

                    //Remove unselected pathogens
                    for (int i = CurrentResults.Count - 1; i >= 0 ; i--)
                    {
                        var isFound = selectedPathogenItems.Any(x => x.PathogenId == CurrentResults[i].AgentId);
                        if (!isFound)
                        {
                            CurrentResults.RemoveAt(i);
                        }
                    }

                    //Add selected pathogens
                    foreach (var pathogenItem in selectedPathogenItems)
                    {
                        var existingResultCell = CurrentResults.FirstOrDefault(x=> x.AgentId == pathogenItem.PathogenId);
                        if(existingResultCell == null)
                        {
                            CurrentResults.Add(new ResultCell
                            {
                                ActivityId = SelectedActivity.activity_id,
                                CompositeSampleId = SelectedMaterial.composite_sample_id,
                                AgentId = pathogenItem.PathogenId,
                                NumberResult = null,
                                NewNumberResult = null,
                                PathogenName = pathogenItem.PathogenName,
                                SeedsCount = "0",
                                SeedsMaxCount = SeedsMaxCount,
                                Enabled = SeedsMaxCount > 0,
                                AuxiliaryResult = "info",
                                TextResult = null,
                            });
                        }
                    }

                    //if(SelectedPathogenItemList.Any())
                    //{
                    //    foreach (var item in selectedPathogenItems)
                    //    {
                    //        var oldItem = SelectedPathogenItemList.FirstOrDefault(x => x.PathogenId == item.PathogenId);
                    //        if (oldItem != null)
                    //        {
                    //            item.SeedsCount = oldItem.SeedsCount;
                    //        }
                    //    }
                    //}
                    //SelectedPathogenItemList = new ObservableCollection<PathogenItem>(selectedPathogenItems);
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand PreviousMaterialCommand { get; }
        public async void OnPreviousMaterialCommand()
        {
            try
            {
                if (SelectedMaterial.num_order <= 1)
                {
                    return;
                }
                IsBusy = true;

                var previousMaterial = MaterialList.FirstOrDefault(x => x.num_order == SelectedMaterial.num_order - 1);
                if (previousMaterial == null)
                    throw new Exception("Next material not found");

                SelectedMaterial = previousMaterial;
                SelectedActivity = ActivityList.First();

                await loadEvaluationItem();

                IsBusy = false;
            }
            catch (Exception ex)
            {
                CurrentResults = null;
                SeedsMaxCount = 0;
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        public DelegateCommand NextMaterialCommand { get; }
        public async void OnNextMaterialCommand()
        {
            try
            {
                if (SelectedMaterial.num_order >= MaterialList.Count())
                {
                    return;
                }
                IsBusy = true;

                var nextMaterial = MaterialList.FirstOrDefault(x => x.num_order == SelectedMaterial.num_order + 1);
                if (nextMaterial == null)
                    throw new Exception("Next material not found");

                SelectedMaterial = nextMaterial;
                SelectedActivity = ActivityList.First();

                await loadEvaluationItem();

                IsBusy = false;
            }
            catch (Exception ex)
            {
                CurrentResults = null;
                SeedsMaxCount = 0;
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        public DelegateCommand PreviousActivityCommand { get; }
        private async void OnPreviousActivityCommand()
        {
            try
            {
                if (SelectedActivity.activity_order <= 1)
                {
                    return;
                }
                IsBusy = true;

                var previousActivity = ActivityList.FirstOrDefault(x => x.activity_order == SelectedActivity.activity_order - 1);
                if (previousActivity == null)
                    throw new Exception("Previous repetition not found");

                SelectedActivity = previousActivity;

                await loadEvaluationItem();

                IsBusy = false;
            }
            catch (Exception ex)
            {
                CurrentResults = null;
                SeedsMaxCount = 0;
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        public DelegateCommand NextActivityCommand { get; }
        private async void OnNextActivityCommand()
        {
            try
            {
                if (SelectedActivity.activity_order >= ActivityList.Count())
                {
                    return;
                }
                IsBusy = true;

                var nextActivity = ActivityList.FirstOrDefault(x => x.activity_order == SelectedActivity.activity_order + 1);
                if (nextActivity == null)
                    throw new Exception("Next repetition not found");

                SelectedActivity = nextActivity;

                await loadEvaluationItem();

                IsBusy = false;
            }
            catch (Exception ex)
            {
                CurrentResults = null;
                SeedsMaxCount = 0;
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        public DelegateCommand SaveEvaluationCommmand { get; }
        private async void OnSaveEvaluationCommmand()
        {
            try
            {
                IsBusy = true;

                foreach (var item in CurrentResults.Where(x => x.Modified))
                {
                    if(item.NewNumberResult.GetValueOrDefault(0) > SeedsMaxCount)
                    {
                        throw new Exception($"Seed count cant not be more than tested seeds count ({SeedsMaxCount})");
                    }
                }

                foreach (var item in CurrentResults.Where(x=>x.Modified))
                {
                    await _requestService.CreateResult(SelectedSampleType.request_process_essay_id, new Result()
                    {
                        activity_id = item.ActivityId,
                        composite_sample_id = item.CompositeSampleId,
                        agent_id = item.AgentId,
                        auxiliary_result = item.AuxiliaryResult,
                        number_result = item.NewNumberResult,
                        text_result = item.TextResult,
                        reprocess = MarkedToReprocess,
                    }, SelectedSampleType.IsAvailableOffline);

                    EventAggregator.GetEvent<RequestProcessAssayActivitySampleUpdatedEvent>().Publish();
                }
                /*
                foreach (var pathogen in SelectedPathogenItemList)
                {
                    // new Result() { number_result = int.Parse(pathogen.SeedsCount), auxiliary_result = "pos" }
                    await _requestService.CreateResult(SelectedSampleType.request_process_essay_id, new Result()
                    {
                        activity_id = SelectedActivity.activity_id,
                        composite_sample_id = SelectedMaterial.composite_sample_id,
                        agent_id = pathogen.PathogenId,
                        auxiliary_result = "info",
                        number_result = int.Parse(pathogen.SeedsCount),
                        text_result = null
                    }, SelectedSampleType.IsAvailableOffline);
                }
                */

                IsBusy = false;
                _toastService.ShowToast("Saved succesfully");
                //await PageDialogService.DisplayAlertAsync("Message", "All results were saved succesfully", "OK");
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        public DelegateCommand FinishEvaluationCommand { get; }
        private async void OnFinishEvaluationCommand()
        {
            try
            {
                IsBusy = true;

                var result = await PageDialogService.DisplayAlertAsync("Warning", "Are you sure to finish this evaluation?" + Environment.NewLine
                    + "You will not be able to modify or enter new results", "Yes", "No");
                if (!result)
                    return;

                SelectedAssaySample.status_id = (int)StatusEnum.Finished;

                var apiResult = await _requestService.UpdateRequestProcessAssay(SelectedSampleType.request_process_essay_id, SelectedAssaySample);
                if (apiResult.Status == 200)
                {
                    await PageDialogService.DisplayAlertAsync("Message", $"Evaluation for {SelectedSampleType.sample_type_name} is finished", "OK");
                    await NavigationService.GoBackToRootAsync();
                }
                else
                {
                    throw new Exception(apiResult.Message);
                }

                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async Task loadEvaluationItem()
        {
            var activitySamples = await _requestService.GetAssayActivitySamples(SelectedSampleType.request_process_essay_id, SelectedSampleType.IsAvailableOffline,
                SelectedActivity.activity_id, SelectedMaterial.composite_sample_id);
            var currentActivitySample = activitySamples.FirstOrDefault();
            if (currentActivitySample == null)
                throw new Exception("Number of tested seeds count is not defined");
            if (currentActivitySample.number_of_seeds == null)
                throw new Exception("Number of tested seeds count is not defined");

            SeedsMaxCount = currentActivitySample.number_of_seeds.Value;

            var tempResults = await _requestService.GetResults(SelectedSampleType.request_process_essay_id, SelectedActivity.activity_id, SelectedMaterial.composite_sample_id, SelectedSampleType.IsAvailableOffline);

            MarkedToReprocess = tempResults.Any() && tempResults.Any(x => x.reprocess);

            var tempCurrentResults = tempResults.Select(x => new ResultCell
            {
                ActivityId = x.activity_id,
                CompositeSampleId = x.composite_sample_id,
                AgentId = x.agent_id,
                NumberResult = x.number_result,
                NewNumberResult = null,
                PathogenName = PathogenItemList.First(p => p.PathogenId == x.agent_id).PathogenName,
                SeedsCount = x.number_result == null ? string.Empty : x.number_result.ToString(),
                SeedsMaxCount = currentActivitySample.number_of_seeds == null ? 0 : currentActivitySample.number_of_seeds.Value,
                Enabled = currentActivitySample.number_of_seeds != null,
                AuxiliaryResult = x.auxiliary_result,
                TextResult = null,
            }).ToList();

            if (KeepPathogens)
            {
                foreach (var selectedPathogenItem in PathogenItemList.Where(x => x.IsSelected))
                {
                    if(!tempCurrentResults.Any(x => x.AgentId == selectedPathogenItem.PathogenId))
                    {
                        tempCurrentResults.Add(new ResultCell
                        {
                            ActivityId = SelectedActivity.activity_id,
                            CompositeSampleId = SelectedMaterial.composite_sample_id,
                            AgentId = selectedPathogenItem.PathogenId,
                            NumberResult = null,
                            NewNumberResult = null,
                            PathogenName = selectedPathogenItem.PathogenName,
                            SeedsCount = "0",
                            SeedsMaxCount = currentActivitySample.number_of_seeds == null ? 0 : currentActivitySample.number_of_seeds.Value,
                            Enabled = currentActivitySample.number_of_seeds != null,
                            AuxiliaryResult = "info",
                            TextResult = null,
                        });
                    }
                }
            }

            foreach (var pathogenItem in PathogenItemList)
            {
                pathogenItem.IsSelected = tempCurrentResults.Any(x => x.AgentId == pathogenItem.PathogenId);
            }

            CurrentResults = new ObservableCollection<ResultCell>(tempCurrentResults);
        }
        public override async void Initialize(INavigationParameters parameters)
        {
            try
            {
                IsBusy = true;

                if (parameters.ContainsKey("Request"))
                {
                    Request = (Request)parameters["Request"];
                }

                if (parameters.ContainsKey("Assay"))
                {
                    Essay = (Assay)parameters["Assay"];
                }

                if (parameters.ContainsKey("SelectedActiveAssaySampleType"))
                {
                    SelectedSampleType = (SampleType)parameters["SelectedActiveAssaySampleType"];
                }

                if (parameters.ContainsKey("SelectedSampleType"))
                {
                    SelectedSampleType = (SampleType)parameters["SelectedSampleType"];
                }

                if (parameters.ContainsKey("SelectedActivity"))
                {
                    SelectedActivity = (Activity)parameters["SelectedActivity"];
                }
                
                if (parameters.ContainsKey("MaterialList"))
                {
                    MaterialList = (IEnumerable<SampleMaterial>)parameters["MaterialList"];

                    if (MaterialList.Any())
                    {
                        SelectedMaterial = MaterialList.FirstOrDefault();
                    }
                }

                SelectedPathogenItemList = new ObservableCollection<PathogenItem>();

                var agents = await _requestService.GetAgents(Request.request_id, SelectedSampleType.process_id, Essay.assay_id, SelectedSampleType.sample_type_id);

                var tempPathogenItemList = new List<PathogenItem>();
                foreach (var agent in agents)
                {
                    tempPathogenItemList.Add(new PathogenItem()
                    {
                        PathogenId = agent.agent_id,
                        PathogenName = agent.agent_name
                    });
                }
                PathogenItemList = new ObservableCollection<PathogenItem>(tempPathogenItemList);

                if(MaterialList == null)
                {
                    var materials = await _requestService.GetRequestProcessAssaySamples(SelectedSampleType.request_process_essay_id);
                    
                    var tempMaterialList = materials.ToList();
                    MaterialList = tempMaterialList.Where(x => x.num_order.HasValue);

                    if (MaterialList.Any())
                    {
                        SelectedMaterial = MaterialList.FirstOrDefault();
                    }
                }

                if(ActivityList == null)
                {
                    ActivityList = SelectedSampleType.activity_list;
                    if (ActivityList.Any())
                        SelectedActivity = ActivityList.FirstOrDefault();
                }

                await loadEvaluationItem();

                IsBusy = false;
            }
            catch (Exception ex)
            {
                CurrentResults = null;
                SeedsMaxCount = 0;
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
    }
}
