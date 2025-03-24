using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SeedHealthApp.Custom.Events;
using SeedHealthApp.Models;
using SeedHealthApp.Models.Dtos;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SeedHealthApp.ViewModels
{
    public class AssayPreparationPageViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;
        private readonly IToastService _toastService;
        private IEnumerable<SampleMaterial> _allSamples;
        private int _pageSize;
        private int _pagesCount;
        private int _maxSeedCount;

        #region properties
        private SampleType _SelectedSampleType;
        public SampleType SelectedSampleType
        {
            get { return _SelectedSampleType; }
            set { SetProperty(ref _SelectedSampleType, value); }
        }
        //public SampleType SelectedSampleType { get; set; }
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
        private int _activityNumber;
        public int ActivityNumber
        {
            get { return _activityNumber; }
            set { SetProperty(ref _activityNumber, value); }
        }
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
        private bool _isEvaluationEnable;
        public bool IsEvaluationEnable
        {
            get { return _isEvaluationEnable; }
            set { SetProperty(ref _isEvaluationEnable, value); }
        }

        private ObservableCollection<AssaySampleActivityList> _sampleWithActivitiesList;
        public ObservableCollection<AssaySampleActivityList> SampleWithActivitiesList
        {
            get { return _sampleWithActivitiesList; }
            set { SetProperty(ref _sampleWithActivitiesList, value); }
        }

        private int? _batchSeedsCount;
        public string BatchSeedsCount
        {
            get {
                if (_batchSeedsCount == null) return "";
                else return _batchSeedsCount.ToString();
            }
            set {
                int result;
                var success = int.TryParse(value, out result);
                _batchSeedsCount = success ? (int?)result : null;
                RaisePropertyChanged(nameof(BatchSeedsCount));
            }
        }
        private int _samplesTotalCount;
        public int SamplesTotalCount
        {
            get { return _samplesTotalCount; }
            set { SetProperty(ref _samplesTotalCount, value); }
        }
        public int CurrentSampleListPage { get; set; }
        private int _sampleOrderStart;
        public int SampleOrderStart
        {
            get { return _sampleOrderStart; }
            set { SetProperty(ref _sampleOrderStart, value); }
        }
        private int _sampleOrderEnd;
        public int SampleOrderEnd
        {
            get { return _sampleOrderEnd; }
            set { SetProperty(ref _sampleOrderEnd, value); }
        }

        public bool OnlyEmptyCells { get; set; }
        #endregion
        public AssayPreparationPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IRequestService requestService, IEventAggregator eventAggregator, IToastService toastService)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _requestService = requestService;
            _toastService = toastService;

            PreviousActivityCommand = new DelegateCommand(OnPreviousActivityCommand);
            NextActivityCommand = new DelegateCommand(OnNextActivityCommand);
            SaveTestedSeedsCountCommand = new DelegateCommand(OnSaveTestedSeedsCountCommand);

            FillAllSeedsCountCommand = new DelegateCommand(OnFillAllSeedsCountCommand, () => { return IsBusy == false; });
            PreviousSampleListPageCommand = new DelegateCommand(ExecutePreviousSampleListPageCommand);
            NextSampleListPageCommand = new DelegateCommand(ExecuteNextSampleListPageCommand);

            Title = "Tested seeds count";
            Username = SettingsService.Username;
            OnlyEmptyCells = true;

            ShowDebugInfoCommand = new DelegateCommand(OnShowDebugInfoCommand);
        }
        public DelegateCommand ShowDebugInfoCommand { get; }
        private async void OnShowDebugInfoCommand()
        {
            try
            {
                string message = Newtonsoft.Json.JsonConvert.SerializeObject(_request, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Newtonsoft.Json.JsonConvert.SerializeObject(SelectedSampleType, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Newtonsoft.Json.JsonConvert.SerializeObject(_sampleWithActivitiesList, Newtonsoft.Json.Formatting.Indented);
                await PageDialogService.DisplayAlertAsync("", message, "OK");
            }
            catch (Exception ex)
            {
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

                IsEvaluationEnable = false;

                var previousActivity = SelectedAssaySample.activity_list.FirstOrDefault(x => x.activity_order == SelectedActivity.activity_order - 1);
                if (previousActivity == null)
                    throw new Exception("Next activity not found");

                SelectedActivity = previousActivity;

                //var apiResponse = await _requestService.GetRequestProcessAssaySamples(Request.request_id, SelectedSampleType.process_id, Essay.assay_id, SelectedSampleType.sample_type_id,
                //    SelectedActivity.activity_id, Request.sample_group_id);
                //if (apiResponse.Status == 200)
                //{
                //    bool existsEmptyTestedSeedCount = false;
                //    var tempMaterialList = apiResponse.Data.ToList();
                //    for (int i = tempMaterialList.Count() - 1; i >= 0; i--)
                //    {
                //        if (!tempMaterialList[i].num_order.HasValue)
                //            tempMaterialList.RemoveAt(i);
                //        else if (!tempMaterialList[i].tested_seeds_count.HasValue)
                //            existsEmptyTestedSeedCount = true;
                //    }
                //    MaterialList = tempMaterialList;
                //    IsEvaluationEnable = !existsEmptyTestedSeedCount;
                //}
                //else
                //{
                //    throw new Exception(apiResponse.Message);
                //}

                IsBusy = false;
            }
            catch (Exception ex)
            {
                MaterialList = null;
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        public DelegateCommand NextActivityCommand { get; }
        private async void OnNextActivityCommand()
        {
            try
            {
                if (SelectedActivity.activity_order >= SelectedAssaySample.activity_list.Count())
                {
                    return;
                }

                IsBusy = true;

                IsEvaluationEnable = false;

                var nextActivity = SelectedAssaySample.activity_list.FirstOrDefault(x => x.activity_order == SelectedActivity.activity_order + 1);
                if (nextActivity == null)
                    throw new Exception("Next activity not found");

                SelectedActivity = nextActivity;

                //var apiResponse = await _requestService.GetRequestProcessAssaySamples(Request.request_id, SelectedSampleType.process_id, Essay.assay_id,
                //    SelectedSampleType.sample_type_id, SelectedActivity.activity_id, Request.sample_group_id);
                //if (apiResponse.Status == 200)
                //{
                //    bool existsEmptyTestedSeedCount = false;
                //    var tempMaterialList = apiResponse.Data.ToList();
                //    for (int i = tempMaterialList.Count() - 1; i >= 0; i--)
                //    {
                //        if (!tempMaterialList[i].num_order.HasValue)
                //            tempMaterialList.RemoveAt(i);
                //        else if (!tempMaterialList[i].tested_seeds_count.HasValue)
                //            existsEmptyTestedSeedCount = true;
                //    }
                //    MaterialList = tempMaterialList;
                //    IsEvaluationEnable = !existsEmptyTestedSeedCount;
                //}
                //else
                //{
                //    throw new Exception(apiResponse.Message);
                //}
                IsBusy = false;
            }
            catch (Exception ex)
            {
                MaterialList = null;
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        private async Task Send(SampleMaterial material)
        {
            try
            {
                var apiResponse = await _requestService.UpdateAssaySampleMaterial(Request.request_id, Essay.assay_id, SelectedSampleType.sample_type_id,
                        SelectedActivity.activity_id, Request.sample_group_id, material);
            }
            catch (Exception e)
            {
                //_logger.LogError(e, e.Message);
            }
        }
        public DelegateCommand SaveTestedSeedsCountCommand { get; }
        private async void OnSaveTestedSeedsCountCommand()
        {
            try
            {
                IsBusy = true;

                int ErrorCount = 0;
                string ErrorMessage = string.Empty;

                if(Xamarin.Essentials.DeviceInfo.Idiom == Xamarin.Essentials.DeviceIdiom.Tablet)
                {
                    //Validate max seeds counts
                    foreach (var item in SampleWithActivitiesList)
                    {
                        var found = item.tested_seeds_counts.Any(x => x.Modified && x.NewValue.GetValueOrDefault(0) > _maxSeedCount);
                        if (found)
                            throw new Exception($"Seed count can't be more than {_maxSeedCount}");
                    }

                    if (SelectedSampleType.IsAvailableOffline)
                    {
                        var allModifiedActivitySamples = new List<AssayActivitySample>();
                        var modifiedCells = new List<AssaySampleActivityCell>();
                        foreach (var item in SampleWithActivitiesList)
                        {
                            modifiedCells.AddRange(item.tested_seeds_counts.Where(x => x.Modified));
                            
                            var modifiedActivitySamples = item.tested_seeds_counts.Where(x => x.Modified)
                                .Select(x => new AssayActivitySample
                                {
                                    LocalId = x.EntityId,
                                    request_process_essay_id = SelectedSampleType.request_process_essay_id,
                                    composite_sample_id = x.composite_sample_id,
                                    activity_id = x.activity_id,
                                    number_of_seeds = x.NewValue,
                                    activity_sample_order = item.num_order,
                                })
                                .ToList();
                            allModifiedActivitySamples.AddRange(modifiedActivitySamples);
                        }

                        await _requestService.UpdateAssayActivitySampleMany(allModifiedActivitySamples, true);
                        foreach (var modifiedCell in modifiedCells)
                        {
                            modifiedCell.Value = modifiedCell.NewValue;
                            modifiedCell.Modified = false;
                        }
                    }
                    else
                    {
                        var allModifiedCells = new List<RequestProcessAssayActivitySampleBatch>();
                        foreach (var item in SampleWithActivitiesList)
                        {
                            var modifiedActivitySamples = item.tested_seeds_counts.Where(x => x.Modified)
                                .Select(x => new RequestProcessAssayActivitySampleBatch
                                {
                                    composite_sample_id = x.composite_sample_id,
                                    activity_id = x.activity_id,
                                    number_of_seeds = x.NewValue
                                })
                                .ToList();
                            allModifiedCells.AddRange(modifiedActivitySamples);
                        }
                        var response = await _requestService.UpdateAssayActivitySampleMany(SelectedSampleType.request_process_essay_id,
                        false, allModifiedCells, SelectedSampleType.IsAvailableOffline);
                        if (response.Status != 200)
                            throw new Exception(response.Message);
                    }
                    //await PageDialogService.DisplayAlertAsync("", Newtonsoft.Json.JsonConvert.SerializeObject(allModifiedCells,Newtonsoft.Json.Formatting.Indented), "OK");
                }
                else
                {
                    var block = new ActionBlock<SampleMaterial>(Send, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 10 });

                    foreach (var item in MaterialList)
                    {
                        block.Post(item); // Post all items to the block
                    }

                    block.Complete(); // Signal completion
                    await block.Completion; // Asynchronously wait for completion.
                }
                
                EventAggregator.GetEvent<RequestProcessAssayActivitySampleUpdatedEvent>().Publish();

                IsBusy = false;
                if (ErrorCount > 0)
                    await PageDialogService.DisplayAlertAsync("Error", $"{ErrorCount} error(s) ocurred while saving." + Environment.NewLine +
                        ErrorMessage + "Try to save again please.", "OK");
                else
                {
                    IsEvaluationEnable = true;
                    _toastService.ShowToast("Saved succesfully");
                    //await PageDialogService.DisplayAlertAsync("Message", "All tested seeds counts were successfully saved", "OK");
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        
        public DelegateCommand FillAllSeedsCountCommand { get; }
        private async void OnFillAllSeedsCountCommand()
        {
            try
            {
                IsBusy = true;

                foreach (var item in SampleWithActivitiesList)
                {
                    foreach (var item2 in item.tested_seeds_counts)
                    {
                        if (!item2.Enabled)
                            continue;

                        if(!OnlyEmptyCells || (OnlyEmptyCells && string.IsNullOrWhiteSpace(item2.DisplayValue)))
                            item2.DisplayValue = BatchSeedsCount;
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
        public DelegateCommand PreviousSampleListPageCommand { get; }
        private async void ExecutePreviousSampleListPageCommand()
        {
            try
            {
                IsBusy = true;

                if (CurrentSampleListPage <= 1)
                    return;

                if (CheckUnsavedChanges())
                {
                    var continueWithOutSaving = await PageDialogService.DisplayAlertAsync("Warning", "There are some unsaved changes. Are you sure to continue?", "Yes", "No");
                    if (!continueWithOutSaving)
                        return;
                }

                CurrentSampleListPage--;
                SampleOrderStart = (CurrentSampleListPage - 1) * _pageSize + 1;
                SampleOrderEnd = Math.Min(CurrentSampleListPage * _pageSize, SamplesTotalCount);

                MaterialList = _allSamples.Skip((CurrentSampleListPage - 1) * _pageSize).Take(_pageSize);

                await LoadRequestAssayActivitySamples();
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
        public DelegateCommand NextSampleListPageCommand { get; }
        private async void ExecuteNextSampleListPageCommand()
        {
            try
            {
                IsBusy = true;

                if (CurrentSampleListPage >= _pagesCount)
                    return;

                if (CheckUnsavedChanges())
                {
                    var continueWithOutSaving = await PageDialogService.DisplayAlertAsync("Warning", "There are some unsaved changes. Are you sure to continue?", "Yes", "No");
                    if (!continueWithOutSaving)
                        return;
                }

                CurrentSampleListPage++;
                SampleOrderStart = (CurrentSampleListPage - 1) * _pageSize + 1;
                SampleOrderEnd = Math.Min(CurrentSampleListPage * _pageSize, SamplesTotalCount);

                MaterialList = _allSamples.Skip((CurrentSampleListPage - 1) * _pageSize).Take(_pageSize);

                await LoadRequestAssayActivitySamples();
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
        private bool CheckUnsavedChanges()
        {
            var AnyModified = false;
            foreach (var item in SampleWithActivitiesList)
            {
                AnyModified = item.tested_seeds_counts.Any(x => x.Modified);
                if (AnyModified)
                {
                    break;
                }
            }
            return AnyModified;
        }
        public override async void OnNavigatedTo(INavigationParameters parameters)
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

                    if (Essay.assay_id == (int)AssayMobileEnum.GerminationTest)
                    {
                        if (Request.crop_id == 3) //Maize
                        {
                            _maxSeedCount = 50;
                        }
                        else if (Request.crop_id == 4) //Wheet
                        {
                            _maxSeedCount = 100;
                        }
                    }
                    else if (Essay.assay_id == (int)AssayMobileEnum.BlotterAndFreezePaperTest)
                    {
                        if (Request.crop_id == 3)
                        {
                            _maxSeedCount = 40;
                        }
                        else if (Request.crop_id == 4)
                        {
                            _maxSeedCount = 80;
                        }
                    }
                    if(_maxSeedCount > 0)
                    {
                        BatchSeedsCount = _maxSeedCount.ToString();
                    }
                }

                if (parameters.ContainsKey("SelectedSampleType"))
                {
                    SelectedSampleType = (SampleType)parameters["SelectedSampleType"];
                }

                if (parameters.ContainsKey("SelectedActiveAssaySampleType"))
                {
                    SelectedSampleType = (SampleType)parameters["SelectedActiveAssaySampleType"];

                    var activityList = SelectedSampleType.activity_list;

                    if (activityList == null || !activityList.Any())
                        throw new Exception("Assay sample does not have repetitions");

                    if (Xamarin.Essentials.DeviceInfo.Idiom == Xamarin.Essentials.DeviceIdiom.Tablet)
                    {
                        var allSamples = await _requestService.GetRequestProcessAssaySamples(SelectedSampleType.request_process_essay_id);
                        _allSamples = allSamples.Where(x => x.num_order.HasValue).ToList();

                        SamplesTotalCount = _allSamples.Count();

                        _pageSize = 100;
                        _pagesCount = (int)Math.Ceiling(SamplesTotalCount * 1.0 / _pageSize);
                        CurrentSampleListPage = 1;

                        await LoadRequestAssayActivitySamples();
                    }
                    else
                    {
                        SelectedActivity = activityList.First();

                        var materials = await _requestService.GetRequestProcessAssaySamples(SelectedSampleType.request_process_essay_id);
                        
                        var tempMaterialList = materials.ToList();
                        MaterialList = tempMaterialList.Where(x => x.num_order.HasValue);
                    }
                }

                if (SelectedSampleType?.activity_qty != null)
                    Xamarin.Forms.MessagingCenter.Send<string, int>("", "LoadDataGrid", (int)SelectedSampleType.activity_qty);

                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        private async Task LoadRequestAssayActivitySamples()
        {
            SampleOrderStart = (CurrentSampleListPage - 1) * _pageSize + 1;
            SampleOrderEnd = Math.Min(CurrentSampleListPage * _pageSize, SamplesTotalCount);
            MaterialList = _allSamples.Skip((CurrentSampleListPage - 1) * _pageSize).Take(_pageSize);

            var tempAssaySampleWithActivitiesList = new List<AssaySampleActivityList>();
            foreach (var sampleMaterial in MaterialList)
            {
                var tempAssaySampleActivityCellList = new List<AssaySampleActivityCell>();
                for (int i = 0; i < SelectedSampleType.activity_qty; i++)
                {
                    tempAssaySampleActivityCellList.Add(new AssaySampleActivityCell()
                    {
                        Enabled = false
                    });
                }
                tempAssaySampleWithActivitiesList.Add(new AssaySampleActivityList
                {
                    composite_sample_id = sampleMaterial.composite_sample_id,
                    composite_sample_name = sampleMaterial.composite_sample_name,
                    num_order = sampleMaterial.num_order,
                    tested_seeds_counts = tempAssaySampleActivityCellList
                });
            }

            var tempAssayActivitySamples = await _requestService.GetAssayActivitySamples(SelectedSampleType.request_process_essay_id, SelectedSampleType.IsAvailableOffline, null, null,
                SampleOrderStart - 1, _pageSize);
            foreach (var sampleItem in tempAssaySampleWithActivitiesList)
            {
                var activityIndex = 0;
                foreach (var activity in SelectedSampleType.activity_list)
                {
                    var assayActivitySample = tempAssayActivitySamples
                        .FirstOrDefault(x => x.composite_sample_id == sampleItem.composite_sample_id && x.activity_id == activity.activity_id);
                    if (assayActivitySample != null)
                    {
                        sampleItem.tested_seeds_counts.ElementAt(activityIndex).composite_sample_id = assayActivitySample.composite_sample_id;
                        sampleItem.tested_seeds_counts.ElementAt(activityIndex).activity_id = assayActivitySample.activity_id;
                        sampleItem.tested_seeds_counts.ElementAt(activityIndex).Value = assayActivitySample.number_of_seeds;
                        sampleItem.tested_seeds_counts.ElementAt(activityIndex).DisplayValue = assayActivitySample.number_of_seeds?.ToString();
                        sampleItem.tested_seeds_counts.ElementAt(activityIndex).Enabled = true;
                        sampleItem.tested_seeds_counts.ElementAt(activityIndex).EntityId = assayActivitySample.LocalId;
                    }
                    activityIndex++;
                }
            }

            SampleWithActivitiesList = new ObservableCollection<AssaySampleActivityList>(tempAssaySampleWithActivitiesList);
        }
    }
}
