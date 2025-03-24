using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
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
    public class ResultsGerminationTestPageViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;
        private readonly IToastService _toastService;
        private SampleType SelectedSampleType;

        private IEnumerable<SampleMaterial> _materialList;
        public IEnumerable<SampleMaterial> MaterialList
        {
            get { return _materialList; }
            set { SetProperty(ref _materialList, value); }
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

        private IEnumerable<GerminationTestAgentItem> _germinationTestAgentItemList;
        public IEnumerable<GerminationTestAgentItem> GerminationTestAgentItemList
        {
            get { return _germinationTestAgentItemList; }
            set { SetProperty(ref _germinationTestAgentItemList, value); }
        }

        private SampleMaterial _selectedMaterial;
        public SampleMaterial SelectedMaterial
        {
            get { return _selectedMaterial; }
            set { SetProperty(ref _selectedMaterial, value); }
        }

        private GerminationTestAgentItem _symptomsNumber;
        public GerminationTestAgentItem SymptomsNumber
        {
            get { return _symptomsNumber; }
            set { SetProperty(ref _symptomsNumber, value); }
        }
        private GerminationTestAgentItem _symptomsType;
        public GerminationTestAgentItem SymptomsType
        {
            get { return _symptomsType; }
            set { SetProperty(ref _symptomsType, value); }
        }
        private int _seedsMaxCount;
        public int SeedsMaxCount
        {
            get { return _seedsMaxCount; }
            set { SetProperty(ref _seedsMaxCount, value); }
        }
        private ObservableCollection<ResultCell> _currentResults;
        public ObservableCollection<ResultCell> CurrentResults
        {
            get { return _currentResults; }
            set { SetProperty(ref _currentResults, value); }
        }
        private ResultCell _symthompSeedsCountCell;
        public ResultCell SymthompSeedsCountCell
        {
            get { return _symthompSeedsCountCell; }
            set { SetProperty(ref _symthompSeedsCountCell, value); }
        }
        private ResultCell _symptomDescriptionCell;
        public ResultCell SymptomDescriptionCell
        {
            get { return _symptomDescriptionCell; }
            set { SetProperty(ref _symptomDescriptionCell, value); }
        }
        private int _subProcessNumber;
        public int SubProcessNumber
        {
            get { return _subProcessNumber; }
            set { SetProperty(ref _subProcessNumber, value); }
        }
        private bool _isSubProcess2;
        public bool IsSubProcess2
        {
            get { return _isSubProcess2; }
            set { SetProperty(ref _isSubProcess2, value); }
        }
        private bool _markedToReprocess;
        public bool MarkedToReprocess
        {
            get { return _markedToReprocess; }
            set { SetProperty(ref _markedToReprocess, value); }
        }
        private ResultCell _normalSeedsCell;
        public ResultCell NormalSeedsCell
        {
            get { return _normalSeedsCell; }
            set { SetProperty(ref _normalSeedsCell, value); }
        }
        private ResultCell _abnormalSeedsCell;
        public ResultCell AbnormalSeedsCell
        {
            get { return _abnormalSeedsCell; }
            set { SetProperty(ref _abnormalSeedsCell, value); }
        }
        private ResultCell _notGerminatedSeedsCell;
        public ResultCell NotGerminatedSeedsCell
        {
            get { return _notGerminatedSeedsCell; }
            set { SetProperty(ref _notGerminatedSeedsCell, value); }
        }
        public ResultsGerminationTestPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IRequestService requestService, IEventAggregator eventAggregator, IToastService toastService)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _requestService = requestService;
            _toastService = toastService;

            PreviousMaterialCommand = new DelegateCommand(OnPreviousMaterialCommand);
            NextMaterialCommand = new DelegateCommand(OnNextMaterialCommand);

            PreviousActivityCommand = new DelegateCommand(ExecPreviousActivityCommand);
            NextActivityCommand = new DelegateCommand(ExecNextActivityCommand);

            SaveEvaluationCommmand = new DelegateCommand(OnSaveEvaluationCommmand);
            UpdateNotGerminatedCellCommand = new DelegateCommand(ExecuteUpdateNotGerminatedCellCommand).ObservesProperty(() => IsIdle);

            Title = "Germination Test";
            Username = SettingsService.Username;

        }

        private async void ExecNextActivityCommand()
        {
            try
            {
                IsBusy = true;

                if (SelectedActivity.activity_order >= ActivityList.Count())
                {
                    return;
                }

                var nextActivity = ActivityList.FirstOrDefault(x => x.activity_order == SelectedActivity.activity_order + 1);
                if (nextActivity == null)
                    throw new Exception("Next repetition not found");

                SelectedActivity = nextActivity;

                await loadEvaluationItem();
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

        private async void ExecPreviousActivityCommand()
        {
            try
            {
                IsBusy = true;

                if (SelectedActivity.activity_order <= 1)
                {
                    return;
                }

                var previousActivity = ActivityList.FirstOrDefault(x => x.activity_order == SelectedActivity.activity_order - 1);
                if (previousActivity == null)
                    throw new Exception("Previous repetition not found");

                SelectedActivity = previousActivity;

                await loadEvaluationItem();
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

        public DelegateCommand PreviousMaterialCommand { get; }
        private async void OnPreviousMaterialCommand()
        {
            try
            {
                IsBusy = true;

                if (SelectedMaterial.num_order <= 1)
                {
                    return;
                }

                var previousMaterial = MaterialList.FirstOrDefault(x => x.num_order == SelectedMaterial.num_order - 1);
                if (previousMaterial == null)
                    throw new Exception("Next material not found");

                SelectedMaterial = previousMaterial;
                SelectedActivity = ActivityList.First();

                await loadEvaluationItem();
                //var previousMaterial = MaterialList.FirstOrDefault(x => x.num_order == SelectedMaterial.num_order - 1);
                //if (previousMaterial == null)
                //    throw new Exception("Next material not found");

                //SelectedMaterial = previousMaterial;
                //foreach (var agent in GerminationTestAgentItemList)
                //{
                //    agent.SeedsCount = "";
                //}
                //SymptomsNumber.SeedsCount = "";
                //SymptomsType.SeedsCount = "";

                //IsBusy = false;
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
        public DelegateCommand NextMaterialCommand { get; }
        public DelegateCommand PreviousActivityCommand { get; }
        public DelegateCommand NextActivityCommand { get; }

        private async void OnNextMaterialCommand()
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

                //var nextMaterial = MaterialList.FirstOrDefault(x => x.num_order == SelectedMaterial.num_order + 1);
                //if (nextMaterial == null)
                //    throw new Exception("Next material not found");

                //SelectedMaterial = nextMaterial;
                //foreach (var agent in GerminationTestAgentItemList)
                //{
                //    agent.SeedsCount = "";
                //}
                //SymptomsNumber.SeedsCount = "";
                //SymptomsType.SeedsCount = "";

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
        public DelegateCommand SaveEvaluationCommmand { get; }
        private async void OnSaveEvaluationCommmand()
        {
            try
            {
                IsBusy = true;
                if (IsSubProcess2)
                {
                    foreach (var item in CurrentResults.Where(x => x.Modified))
                    {
                        if (item.NewNumberResult.GetValueOrDefault(0) > SeedsMaxCount)
                        {
                            throw new Exception($"\"{item.PathogenName}\" cant not be more than tested seeds count ({SeedsMaxCount})");
                        }
                    }
                    var totalCount = CurrentResults.Sum(x => x.NewNumberResult.GetValueOrDefault(0));
                    if (totalCount != SeedsMaxCount)
                        throw new Exception($"The sum of the quantities must be the equal to the count of seeds tested ({SeedsMaxCount})");

                    //Save SeedsCount results
                    foreach (var item in CurrentResults.Where(x => x.Modified))
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

                        EventAggregator.GetEvent<ResultUpdatedEvent>().Publish();
                    }
                }
                else
                {
                    if (SymthompSeedsCountCell.NewNumberResult.GetValueOrDefault(0) > SeedsMaxCount)
                        throw new Exception($"Symthomp seeds count cant not be more than tested seeds count ({SeedsMaxCount})");

                    //Save Symptoms results
                    if (SymthompSeedsCountCell.Modified)
                    {
                        await _requestService.CreateResult(SelectedSampleType.request_process_essay_id, new Result()
                        {
                            activity_id = SymthompSeedsCountCell.ActivityId,
                            composite_sample_id = SymthompSeedsCountCell.CompositeSampleId,
                            agent_id = SymthompSeedsCountCell.AgentId,
                            auxiliary_result = SymthompSeedsCountCell.AuxiliaryResult,
                            number_result = SymthompSeedsCountCell.NewNumberResult,
                            text_result = SymthompSeedsCountCell.TextResult,
                            reprocess = MarkedToReprocess,
                        }, SelectedSampleType.IsAvailableOffline);

                        EventAggregator.GetEvent<ResultUpdatedEvent>().Publish();
                    }
                    if (SymptomDescriptionCell.Modified)
                    {
                        await _requestService.CreateResult(SelectedSampleType.request_process_essay_id, new Result()
                        {
                            activity_id = SymptomDescriptionCell.ActivityId,
                            composite_sample_id = SymptomDescriptionCell.CompositeSampleId,
                            agent_id = SymptomDescriptionCell.AgentId,
                            auxiliary_result = SymptomDescriptionCell.AuxiliaryResult,
                            number_result = SymptomDescriptionCell.NumberResult,
                            text_result = SymptomDescriptionCell.TextDisplayValue,
                            reprocess = MarkedToReprocess,
                        }, SelectedSampleType.IsAvailableOffline);

                        EventAggregator.GetEvent<ResultUpdatedEvent>().Publish();
                    }
                }




                /*
                GerminationTestAgentItem _normalSeeds = null;
                GerminationTestAgentItem _abnormalSeeds = null;
                GerminationTestAgentItem _noGerminatedSeed = null;
                foreach (var agent in GerminationTestAgentItemList)
                {
                    if (agent.AgentId == 48) _normalSeeds = agent;
                    else if (agent.AgentId == 47) _abnormalSeeds = agent;
                    else if (agent.AgentId == 51) _noGerminatedSeed = agent;

                    if (string.IsNullOrEmpty(agent.SeedsCount))
                        agent.SeedsCount = "0";

                    if (int.Parse(agent.SeedsCount) > SelectedMaterial.tested_seeds_count.GetValueOrDefault(0))
                    {
                        throw new Exception($"Seed count cant not be more than tested seeds count ({SelectedMaterial.tested_seeds_count})");
                    }
                }
                if (_normalSeeds == null || _abnormalSeeds == null || _noGerminatedSeed == null)
                    throw new Exception("Required descriptor is missing");

                if (int.Parse(_normalSeeds.SeedsCount) + int.Parse(_abnormalSeeds.SeedsCount) > SelectedMaterial.tested_seeds_count)
                    throw new Exception($"The sum of normal and abnormal seeds exceeds the tested seeds count ({SelectedMaterial.tested_seeds_count})");

                _noGerminatedSeed.SeedsCount = (SelectedMaterial.tested_seeds_count.Value - (int.Parse(_normalSeeds.SeedsCount) + int.Parse(_abnormalSeeds.SeedsCount))).ToString();

                if (string.IsNullOrEmpty(SymptomsNumber.SeedsCount))
                    SymptomsNumber.SeedsCount = "0";

                if (int.Parse(SymptomsNumber.SeedsCount) > SelectedMaterial.tested_seeds_count.GetValueOrDefault(0))
                {
                    throw new Exception($"Seed count cant not be more than tested seeds count ({SelectedMaterial.tested_seeds_count})");
                }

                foreach (var agent in GerminationTestAgentItemList)
                {
                    var apiResponse = await _requestService.UpsertResult(Request.request_id, Essay.assay_id, SelectedSampleType.sample_type_id,
                        SelectedActivity.activity_id, Request.sample_group_id, SelectedMaterial.composite_sample_id, agent.AgentId,
                        new Result() { number_result = int.Parse(agent.SeedsCount), auxiliary_result = "num" });

                    if (apiResponse.Status != 200)
                        throw new Exception(apiResponse.Message);
                }

                var apiResponse2 = await _requestService.UpsertResult(Request.request_id, Essay.assay_id, SelectedSampleType.sample_type_id,
                        SelectedActivity.activity_id, Request.sample_group_id, SelectedMaterial.composite_sample_id, SymptomsNumber.AgentId,
                        new Result() { number_result = int.Parse(SymptomsNumber.SeedsCount), auxiliary_result = "num" });
                if (apiResponse2.Status != 200)
                    throw new Exception(apiResponse2.Message);

                var apiResponse3 = await _requestService.UpsertResult(Request.request_id, Essay.assay_id, SelectedSampleType.sample_type_id,
                        SelectedActivity.activity_id, Request.sample_group_id, SelectedMaterial.composite_sample_id, SymptomsType.AgentId,
                        new Result() { text_result = SymptomsType.SeedsCount, auxiliary_result = "inf" });
                if (apiResponse3.Status != 200)
                    throw new Exception(apiResponse3.Message);
                */

                IsBusy = false;

                _toastService.ShowToast("Saved succesfully");
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        public DelegateCommand UpdateNotGerminatedCellCommand { get; }
        private void ExecuteUpdateNotGerminatedCellCommand()
        {
            try
            {
                IsBusy = true;
                if (NormalSeedsCell != null && AbnormalSeedsCell != null && NotGerminatedSeedsCell != null && SeedsMaxCount > 0)
                {
                    int normalSeedsCount, abnormalSeedsCount;
                    _ = int.TryParse(NormalSeedsCell.SeedsCount, out normalSeedsCount);
                    _ = int.TryParse(AbnormalSeedsCell.SeedsCount, out abnormalSeedsCount);
                    int notGerminatedCount = SeedsMaxCount - normalSeedsCount - abnormalSeedsCount;
                    NotGerminatedSeedsCell.SeedsCount = notGerminatedCount < 0 ? "0" : notGerminatedCount.ToString();
                }
            }
            catch (Exception)
            {
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

                if (parameters.ContainsKey("SubProcessNumber"))
                {
                    SubProcessNumber = parameters.GetValue<int>("SubProcessNumber");
                    IsSubProcess2 = SubProcessNumber == 2;
                }

                if (parameters.ContainsKey("Request"))
                {
                    Request = (Request)parameters["Request"];
                }

                if (parameters.ContainsKey("Essay"))
                {
                    Essay = (Assay)parameters["Essay"];
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

                if (MaterialList == null)
                {
                    var materials = await _requestService.GetRequestProcessAssaySamples(SelectedSampleType.request_process_essay_id);
                    MaterialList = materials.Where(x => x.num_order.HasValue).ToList();

                    if (MaterialList.Any())
                    {
                        SelectedMaterial = MaterialList.FirstOrDefault();
                    }
                }

                if (ActivityList == null)
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
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {

            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        private async Task loadEvaluationItem()
        {
            //Get the maximum seed limit for current sample and repetition
            var activitySamples = await _requestService.GetAssayActivitySamples(SelectedSampleType.request_process_essay_id, SelectedSampleType.IsAvailableOffline,
                SelectedActivity.activity_id, SelectedMaterial.composite_sample_id);
            var currentActivitySample = activitySamples.FirstOrDefault();
            if (currentActivitySample == null)
                throw new Exception("Number of tested seeds count is not defined");
            if (currentActivitySample.number_of_seeds == null)
                throw new Exception("Number of tested seeds count is not defined");
            SeedsMaxCount = currentActivitySample.number_of_seeds.Value;

            var tempResults = await _requestService.GetResults(SelectedSampleType.request_process_essay_id, SelectedActivity.activity_id, SelectedMaterial.composite_sample_id, SelectedSampleType.IsAvailableOffline);
            var agents = await _requestService.GetAgents(Request.request_id, SelectedSampleType.process_id, Essay.assay_id, SelectedSampleType.sample_type_id);

            var seedsCountsAgents = agents.ToList();
            var symthompSeedsCountAgent = seedsCountsAgents.FirstOrDefault(x => x.agent_id == 50);
            //SymptomsNumber = new GerminationTestAgentItem() { AgentId = symthompSeedsCountAgent.agent_id, AgentName = symthompSeedsCountAgent.agent_name };
            var symthompDescriptionAgent = seedsCountsAgents.FirstOrDefault(x => x.agent_id == 82);
            //SymptomsType = new GerminationTestAgentItem() { AgentId = symthompDescriptionAgent.agent_id, AgentName = symthompDescriptionAgent.agent_name };
            seedsCountsAgents.Remove(symthompSeedsCountAgent);
            seedsCountsAgents.Remove(symthompDescriptionAgent);

            MarkedToReprocess = !tempResults.Any() ? true : tempResults.Any(x => x.reprocess);
            //if (!IsSubProcess2)
            //{
            // Seeds Counts Tab
            var tempSeedsCountsResults = seedsCountsAgents.Select(x => new ResultCell
            {
                ActivityId = SelectedActivity.activity_id,
                CompositeSampleId = SelectedMaterial.composite_sample_id,
                AgentId = x.agent_id,
                PathogenName = x.agent_name,
                SeedsMaxCount = currentActivitySample.number_of_seeds == null ? 0 : currentActivitySample.number_of_seeds.Value,
                Enabled = currentActivitySample.number_of_seeds != null,
                SeedsCount = "0",
            }).ToList();

            foreach (var tempSeedsCountCell in tempSeedsCountsResults)
            {
                var found = tempResults.FirstOrDefault(x => x.agent_id == tempSeedsCountCell.AgentId);
                if (found != null)
                {
                    tempSeedsCountCell.NumberResult = found.number_result;
                    tempSeedsCountCell.NewNumberResult = null;
                    tempSeedsCountCell.SeedsCount = found.number_result == null ? "0" : found.number_result.ToString();
                    tempSeedsCountCell.AuxiliaryResult = found.auxiliary_result;
                    tempSeedsCountCell.TextResult = found.text_result;
                }

                if (tempSeedsCountCell.AgentId == 48) //Normal
                {
                    NormalSeedsCell = tempSeedsCountCell;
                }
                else if (tempSeedsCountCell.AgentId == 47) //Abnormal
                {
                    AbnormalSeedsCell = tempSeedsCountCell;
                }
                else if (tempSeedsCountCell.AgentId == 51) //Not germinated
                {
                    NotGerminatedSeedsCell = tempSeedsCountCell;
                }
            }
            CurrentResults = new ObservableCollection<ResultCell>(tempSeedsCountsResults);

            //foreach (var seedsCountAgent in seedsCountsAgents)
            //{
            //    var tempSeedsCountCell = new ResultCell
            //    {
            //        ActivityId = SelectedActivity.activity_id,
            //        CompositeSampleId = SelectedMaterial.composite_sample_id,
            //        AgentId = seedsCountAgent.agent_id,
            //        PathogenName = seedsCountAgent.agent_name,
            //        SeedsMaxCount = currentActivitySample.number_of_seeds == null ? 0 : currentActivitySample.number_of_seeds.Value,
            //        Enabled = currentActivitySample.number_of_seeds != null,
            //        SeedsCount = "0",
            //    };
            //    var savedValue = tempResults.FirstOrDefault(x => x.agent_id == seedsCountAgent.agent_id);
            //    if (savedValue != null)
            //    {
            //        tempSeedsCountCell.NumberResult = savedValue.number_result;
            //        tempSeedsCountCell.NewNumberResult = null;
            //        tempSeedsCountCell.SeedsCount = savedValue.number_result == null ? "0" : savedValue.number_result.ToString();
            //        tempSeedsCountCell.AuxiliaryResult = savedValue.auxiliary_result;
            //        tempSeedsCountCell.TextResult = savedValue.text_result;
            //    }
            //    if (seedsCountAgent.agent_id == 48) //Normal
            //    {
            //        NormalSeedsCell = tempSeedsCountCell;
            //    }
            //    else if (seedsCountAgent.agent_id == 47) //Abnormal
            //    {
            //        AbnormalSeedsCell = tempSeedsCountCell;
            //    }
            //    else if (seedsCountAgent.agent_id == 51) //Not germinated
            //    {
            //        NotGerminatedSeedsCell = tempSeedsCountCell;
            //    }
            //}

            //}
            //else
            //{
            // Symthomps Tab
            var tempSymthompSeedsCountCell = new ResultCell
            {
                ActivityId = SelectedActivity.activity_id,
                CompositeSampleId = SelectedMaterial.composite_sample_id,
                AgentId = symthompSeedsCountAgent.agent_id,
                PathogenName = symthompSeedsCountAgent.agent_name,
                SeedsMaxCount = currentActivitySample.number_of_seeds == null ? 0 : currentActivitySample.number_of_seeds.Value,
                Enabled = currentActivitySample.number_of_seeds != null,
                SeedsCount = "0"
            };
            var symthompResult = tempResults.FirstOrDefault(x => x.agent_id == symthompSeedsCountAgent.agent_id);
            if (symthompResult != null)
            {
                tempSymthompSeedsCountCell.NumberResult = symthompResult.number_result;
                tempSymthompSeedsCountCell.NewNumberResult = null;
                tempSymthompSeedsCountCell.SeedsCount = symthompResult.number_result == null ? string.Empty : symthompResult.number_result.ToString();
                tempSymthompSeedsCountCell.AuxiliaryResult = symthompResult.auxiliary_result;
                tempSymthompSeedsCountCell.TextResult = symthompResult.text_result;
            }
            SymthompSeedsCountCell = tempSymthompSeedsCountCell;

            var tempSymthompDescriptionCell = new ResultCell
            {
                ActivityId = SelectedActivity.activity_id,
                CompositeSampleId = SelectedMaterial.composite_sample_id,
                AgentId = symthompDescriptionAgent.agent_id,
                PathogenName = symthompDescriptionAgent.agent_name,
                SeedsMaxCount = currentActivitySample.number_of_seeds == null ? 0 : currentActivitySample.number_of_seeds.Value,
                Enabled = currentActivitySample.number_of_seeds != null,
                TextDisplayValue = "NA",
            };
            var symthompDescriptionResult = tempResults.FirstOrDefault(x => x.agent_id == symthompDescriptionAgent.agent_id);
            if (symthompDescriptionResult != null)
            {
                tempSymthompDescriptionCell.NumberResult = symthompDescriptionResult.number_result;
                tempSymthompDescriptionCell.NewNumberResult = null;
                //tempSymthompDescriptionCell.SeedsCount = symthompDescriptionResult.number_result == null ? string.Empty : symthompDescriptionResult.number_result.ToString();
                tempSymthompDescriptionCell.AuxiliaryResult = symthompDescriptionResult.auxiliary_result;
                tempSymthompDescriptionCell.TextResult = symthompDescriptionResult.text_result;
                tempSymthompDescriptionCell.TextDisplayValue = symthompDescriptionResult.text_result;
            }
            SymptomDescriptionCell = tempSymthompDescriptionCell;
            //}
        }
    }
}
