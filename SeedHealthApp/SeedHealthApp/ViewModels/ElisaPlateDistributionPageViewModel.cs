using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Prism.Services.Dialogs;
using SeedHealthApp.Custom.Events;
using SeedHealthApp.Extensions;
using SeedHealthApp.Helpers;
using SeedHealthApp.Models;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeedHealthApp.ViewModels
{
    public class ElisaPlateDistributionPageViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;
        private readonly IDialogService _dialogService;
        private ElisaEvent _elisaEvent;
        private IEnumerable<SupportLocation> _platesDistributions = Enumerable.Empty<SupportLocation>();
        private IEnumerable<SampleType> _requestProcessAssayList;
        private int _pathogenId;
        private int[] _pathogenIds = new int[] { };
        private string _pathogenName;
        private string[] _pathogenNames = new string[] { };
        private int _plateOrder;
        private Dictionary<int, string> _requestColorPalette = new Dictionary<int, string>();
        private readonly string _positiveBackgroundColor = "#ffc4d6";
        private readonly string _negativeBackgroundColor = "#d0f4de";
        private readonly string _bufferBackgroundColor = "#adb5bd";
        private List<SupportLocation> _cachePathogenSamples = new List<SupportLocation>();
        private Dictionary<int, Dictionary<string, SampleMaterial>> _TokenSampleCache = new Dictionary<int, Dictionary<string, SampleMaterial>>();
        private Dictionary<int, List<IEnumerable<PlateLocation>>> _pathogenPlates = new Dictionary<int, List<IEnumerable<PlateLocation>>>();

        public IEnumerable<SampleType> RequestProcessAssayList
        {
            get { return _requestProcessAssayList; }
            set { SetProperty(ref _requestProcessAssayList, value); }
        }
        private SampleType _selectedRequestProcessAssay;
        public SampleType SelectedRequestProcessAssay
        {
            get { return _selectedRequestProcessAssay; }
            set { SetProperty(ref _selectedRequestProcessAssay, value); }
        }
        private Dictionary<string, string> _colorLegend;
        public Dictionary<string, string> ColorLegend
        {
            get { return _colorLegend; }
            set { SetProperty(ref _colorLegend, value); }
        }
        private IEnumerable<PlateLocation> _locationList;
        public IEnumerable<PlateLocation> LocationList
        {
            get { return _locationList; }
            set { SetProperty(ref _locationList, value); }
        }
        private List<SampleMaterial> _sampleList;
        public List<SampleMaterial> SampleList
        {
            get { return _sampleList; }
            set { SetProperty(ref _sampleList, value); }
        }
        //private SampleMaterial _selectedSample;
        //public SampleMaterial SelectedSample
        //{
        //    get { return _selectedSample; }
        //    set { SetProperty(ref _selectedSample, value); }
        //}
        private bool _removingCells;
        public bool RemovingCells
        {
            get { return _removingCells; }
            set { SetProperty(ref _removingCells, value); }
        }
        private ReadingDataEntry _selectedEntryType;
        public ReadingDataEntry SelectedEntryType
        {
            get { return _selectedEntryType; }
            set { SetProperty(ref _selectedEntryType, value); }
        }
        private bool _allPathogensIsChecked;
        public bool AllPathogensIsChecked //Multiple selection mode
        {
            get { return _allPathogensIsChecked; }
            set { SetProperty(ref _allPathogensIsChecked, value); }
        }
        private int _sampleIndex;
        public int SampleIndex
        {
            get { return _sampleIndex; }
            set { SetProperty(ref _sampleIndex, value); }
        }
        private List<SelectableModel<SampleMaterial>> _selectableSampleList;
        public List<SelectableModel<SampleMaterial>> SelectableSampleList
        {
            get { return _selectableSampleList; }
            set { SetProperty(ref _selectableSampleList, value); }
        }
        private SelectableModel<SampleMaterial> _selectedSelectableSample;
        public SelectableModel<SampleMaterial> SelectedSelectableSample
        {
            get { return _selectedSelectableSample; }
            set { SetProperty(ref _selectedSelectableSample, value); }
        }

        public bool IsMultiplePathogensMode { get; set; }
        public Dictionary<int, int> RequestProcessAssayLastSamples { get; set; } = new Dictionary<int, int>();
        public ElisaPlateDistributionPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService,
            ISettingsService settingsService, IEventAggregator eventAggregator,
            IRequestService requestService, IDialogService dialogService)
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _requestService = requestService;
            _dialogService = dialogService;
            LocationList = Enumerable.Empty<PlateLocation>();

            RefreshSampleListCommand = new DelegateCommand(ExecuteRefreshSampleListCommand).ObservesCanExecute(() => IsIdle);
            PreviousSampleCommand = new DelegateCommand(ExecutePreviousSampleCommand).ObservesCanExecute(() => IsIdle);
            NextSampleCommand = new DelegateCommand(ExecuteNextSampleCommand).ObservesCanExecute(() => IsIdle);
            SetRemovingModeCommand = new DelegateCommand<object>(ExecuteSetRemovingModeCommand).ObservesCanExecute(() => IsIdle);
            PickCellCommand = new DelegateCommand<object>(OnPickCellCommand).ObservesCanExecute(() => IsIdle);
            SaveCommand = new DelegateCommand(ExecuteSaveCommand).ObservesCanExecute(() => IsIdle);
            SelectSampleCommand = new DelegateCommand(ExecuteSelectSampleCommand).ObservesCanExecute(() => IsIdle);

            ShowDebugInfoCommand = new DelegateCommand(ExecuteShowDebugInfoCommand).ObservesCanExecute(() => IsIdle);

            SelectedEntryType = ReadingDataEntry.Entry;
        }
        public DelegateCommand ShowDebugInfoCommand { get; }
        private async void ExecuteShowDebugInfoCommand()
        {
            try
            {
                IsBusy = true;

                //var result = await PageDialogService.DisplayActionSheetAsync("ActionSheet", "Cancel", "Destroy", "Option 1", "Option 2");

                var buttons = new IActionSheetButton[]
                {
                    ActionSheetButton.CreateButton("Event", DisplayDebugInfo, _elisaEvent),
                    ActionSheetButton.CreateButton("Request sample type list", DisplayDebugInfo, _requestProcessAssayList),
                    ActionSheetButton.CreateButton("Sample List", DisplayDebugInfo, _selectableSampleList),
                    ActionSheetButton.CreateButton("Pathogen samples map", DisplayDebugInfo, _cachePathogenSamples),
                    ActionSheetButton.CreateButton("Distribution records", DisplayDebugInfo,
                        _platesDistributions.Where(x => x.agent_id == _pathogenId)),
                    ActionSheetButton.CreateButton("Distribution records for selected request sample type", DisplayDebugInfo,
                        _platesDistributions.Where(x => x.agent_id == _pathogenId && x.request_process_essay_id == SelectedRequestProcessAssay.request_process_essay_id)),
                    ActionSheetButton.CreateButton("RequestProcessAssay LastSample", DisplayDebugInfo, RequestProcessAssayLastSamples),
                    //ActionSheetButton.CreateCancelButton("Cancel", WriteLine, "Cancel"),
                };

                await PageDialogService.DisplayActionSheetAsync("ActionSheet with ActionSheetButtons", buttons);
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
        private void DisplayDebugInfo(object info)
        {
            try
            {
                _ = PageDialogService.DisplayAlertAsync(string.Empty, JsonConvert.SerializeObject(info, Formatting.Indented), "OK");
            }
            catch (Exception ex)
            {
                _ = PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand SelectSampleCommand { get; }
        private async void ExecuteSelectSampleCommand()
        {
            try
            {
                IsBusy = true;

                if (SelectedRequestProcessAssay == null || SelectableSampleList == null || !SelectableSampleList.Any())
                    return;

                var dialogResult = await _dialogService.ShowDialogAsync("SamplePickerDialog", new DialogParameters
                {
                    {"SelectableSampleList", SelectableSampleList }
                });

                if (dialogResult.Parameters.ContainsKey("SelectedSelectableSample"))
                {
                    SelectedSelectableSample = dialogResult.Parameters.GetValue<SelectableModel<SampleMaterial>>("SelectedSelectableSample");
                    SampleIndex = SelectableSampleList.IndexOf(SelectedSelectableSample) + 1;
                    UpdateRequesProcessAssayLastSample(SelectedRequestProcessAssay.request_process_essay_id, SelectedSelectableSample.Item.composite_sample_id);
                }
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
        public DelegateCommand RefreshSampleListCommand { get; }
        private async void ExecuteRefreshSampleListCommand()
        {
            try
            {
                IsBusy = true;

                if (SelectedRequestProcessAssay == null)
                    return;

                if (!IsMultiplePathogensMode)
                {
                    SampleList = (List<SampleMaterial>)await _requestService.GetElisaSamplesAsync(SelectedRequestProcessAssay.request_process_essay_id);
                    SelectableSampleList = SampleList.Select(x => new SelectableModel<SampleMaterial>
                    {
                        Item = x,
                        Selected = _cachePathogenSamples.Any(y => y.agent_id == _pathogenId && y.composite_sample_id == x.composite_sample_id)
                    }).ToList();
                }
                else
                {
                    SampleList = (List<SampleMaterial>)await _requestService.GetElisaSamplesAsync(SelectedRequestProcessAssay.request_process_essay_id);
                    SelectableSampleList = SampleList.Select(x => new SelectableModel<SampleMaterial>
                    {
                        Item = x,
                        Selected = _cachePathogenSamples.Any(y => y.agent_id == _pathogenId && y.composite_sample_id == x.composite_sample_id)
                    }).ToList();
                }
                if (!_TokenSampleCache.ContainsKey(SelectedRequestProcessAssay.request_process_essay_id))
                {
                    var controlSamples = await _requestService.GetElisaControlSamplesAsync(SelectedRequestProcessAssay.request_process_essay_id);
                    if (controlSamples.Count() != 3)
                        throw new Exception("All control samples not found");
                    var controlSampleMap = new Dictionary<string, SampleMaterial>();
                    foreach (var controlSample in controlSamples)
                    {
                        controlSampleMap.Add(controlSample.composite_sample_name.ToLowerInvariant(), controlSample);
                    }
                    _TokenSampleCache.Add(SelectedRequestProcessAssay.request_process_essay_id, controlSampleMap);
                }

                if (SelectableSampleList.Any())
                {
                    if (RequestProcessAssayLastSamples.ContainsKey(SelectedRequestProcessAssay.request_process_essay_id))
                    {
                        var lastSample = SelectableSampleList
                            .FirstOrDefault(x => x.Item.composite_sample_id == RequestProcessAssayLastSamples[SelectedRequestProcessAssay.request_process_essay_id]);
                        SelectedSelectableSample = lastSample ?? SelectableSampleList.First();
                        RequestProcessAssayLastSamples[SelectedRequestProcessAssay.request_process_essay_id] = SelectedSelectableSample.Item.composite_sample_id;
                    }
                    else
                    {
                        SelectedSelectableSample = SelectableSampleList.First();
                        RequestProcessAssayLastSamples.Add(SelectedRequestProcessAssay.request_process_essay_id, SelectedSelectableSample.Item.composite_sample_id);
                    }
                }
                else
                    throw new Exception("Sample list is empty");

                //SelectedSelectableSample = SelectableSampleList.Any() ? SelectableSampleList.First() : throw new Exception("Sample list is empty");
            }
            catch (Exception ex)
            {
                SelectedSelectableSample = null;
                SampleList = new List<SampleMaterial>();
                await PageDialogService.DisplayErrorAlertAsync(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
        public DelegateCommand PreviousSampleCommand { get; }
        private async void ExecutePreviousSampleCommand()
        {
            try
            {
                IsBusy = true;

                if (SelectedSelectableSample == null)
                    return;
                var iMaterial = SelectableSampleList.IndexOf(SelectedSelectableSample);
                if (iMaterial >= 1)
                {
                    SelectedSelectableSample = SelectableSampleList[--iMaterial];
                    SampleIndex = iMaterial + 1;
                    UpdateRequesProcessAssayLastSample(SelectedRequestProcessAssay.request_process_essay_id, SelectedSelectableSample.Item.composite_sample_id);
                }
                
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
        public DelegateCommand NextSampleCommand { get; }
        private async void ExecuteNextSampleCommand()
        {
            try
            {
                IsBusy = true;
                if (SelectedSelectableSample == null)
                    return;
                var iMaterial = SelectableSampleList.IndexOf(SelectedSelectableSample);
                if (iMaterial <= SelectableSampleList.Count - 2)
                {
                    SelectedSelectableSample = SelectableSampleList[++iMaterial];
                    SampleIndex = iMaterial + 1;
                    UpdateRequesProcessAssayLastSample(SelectedRequestProcessAssay.request_process_essay_id, SelectedSelectableSample.Item.composite_sample_id);
                }
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
        private void UpdateRequesProcessAssayLastSample(int requestProcessAssayId, int compositeSampleId)
        {
            if (RequestProcessAssayLastSamples.ContainsKey(requestProcessAssayId))
            {
                RequestProcessAssayLastSamples[requestProcessAssayId] = compositeSampleId;
            }
            else
            {
                RequestProcessAssayLastSamples.Add(requestProcessAssayId, compositeSampleId);
            }
        }
        public DelegateCommand<object> SetRemovingModeCommand { get; }
        private void ExecuteSetRemovingModeCommand(object isEnabled)
        {
            IsBusy = true;
            RemovingCells = (bool)isEnabled;
            IsBusy = false;
        }
        public DelegateCommand<object> PickCellCommand { get; }
        private async void OnPickCellCommand(object cellPosition)
        {
            try
            {
                if (!RemovingCells && SelectedEntryType == ReadingDataEntry.Entry)
                {
                    if(SelectedRequestProcessAssay == null)
                        throw new Exception("Request sample type is empty");
                    if (SelectedSelectableSample == null)
                        throw new Exception("Sample is empty");
                }

                var currentCell = cellPosition as PlateLocation;
                PlateLocation pairedCell;
                // Get paired cell based on column order
                int columnOrder = (currentCell.Position - 1) / 8 + 1;
                pairedCell = columnOrder % 2 == 0
                    ? LocationList.First(x => x.Position == currentCell.Position - 8)
                    : LocationList.First(x => x.Position == currentCell.Position + 8);

                //if (_selectedPathogen.PathogenId == 0) //All
                if (IsMultiplePathogensMode)
                {
                    //Check if sample is already located in other pathogens plates
                    //if (!RemovingCells && SelectedEntryType == ReadingDataEntry.Entry)
                    //{
                    //    var isPlacedInAnyPathogen = _cachePathogenSamples.Any(x => x.composite_sample_id == SelectedSelectableSample.Item.composite_sample_id);
                    //    if (isPlacedInAnyPathogen)
                    //        throw new Exception("Can't set current sample location for all pathogens because the sample is already placed in other pathogen plate");
                    //}

                    foreach (var pathogenId in _pathogenIds)
                    {
                        var pathogenCurrentCell = _pathogenPlates[pathogenId][0/*_supportOrder - 1*/].First(x => x.Position == currentCell.Position);
                        var pathogenPairedCell = _pathogenPlates[pathogenId][0/*_supportOrder - 1*/].First(x => x.Position == pairedCell.Position);
                        HandlePick(pathogenCurrentCell, pathogenPairedCell, false, pathogenId);
                    }
                    HandlePick(currentCell, pairedCell, true, 0);
                }
                else
                {
                    HandlePick(currentCell, pairedCell, true, _pathogenId);
                }
            }
            catch (WarningException wEx)
            {
                await PageDialogService.DisplayAlertAsync(string.Empty, wEx.Message, "OK");
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        private void HandlePick(PlateLocation currentCell, PlateLocation pairedCell, bool updateCurrentSample, int pathogenId)
        {
            if (RemovingCells)
            {
                if (currentCell.IsSelected)
                {
                    if (currentCell.DataType == ReadingDataEntry.Entry)
                    {
                        //Remove from cache
                        var cachedPathogenSample = _cachePathogenSamples.FirstOrDefault(x => x.agent_id == pathogenId && x.composite_sample_id == currentCell.composite_sample_id);
                        if (cachedPathogenSample != null)
                            _cachePathogenSamples.Remove(cachedPathogenSample);
                        if (updateCurrentSample && SelectableSampleList != null)
                        {
                            //Unselect material
                            var currentCellMaterial = SelectableSampleList.FirstOrDefault(x => x.Item.composite_sample_id == currentCell.composite_sample_id);
                            //TODO: Maybe save removing cells
                            if (currentCellMaterial != null)
                            {
                                currentCellMaterial.Selected = false;
                            }
                        }
                    }

                    //Unselect cell
                    currentCell.request_process_assay_id = pairedCell.request_process_assay_id = 0;
                    currentCell.composite_sample_id = pairedCell.composite_sample_id = 0;
                    currentCell.request_id = pairedCell.request_id = 0;
                    currentCell.sample_type_id = pairedCell.sample_type_id = 0;
                    currentCell.DataType = pairedCell.DataType = 0;
                    currentCell.IsSelected = pairedCell.IsSelected = false;

                    currentCell.Title = pairedCell.Title = string.Empty;
                    currentCell.Subtitle = pairedCell.Subtitle = string.Empty;
                    currentCell.Color = pairedCell.Color = "White";
                    currentCell.SampleTypeIndicator = pairedCell.SampleTypeIndicator = string.Empty;
                }
            }
            else
            {
                //Check entry type
                if (SelectedEntryType == ReadingDataEntry.Entry)
                {
                    //Check if all samples are located
                    if (!SelectableSampleList.Any(x => !x.Selected))
                        throw new WarningException("All samples are located");

                    //Check if current material is empty and CellPosition is unselected
                    if (!currentCell.IsSelected && SelectedSelectableSample == null)
                    {
                        //Lista de materiales esta vacia o se terminaron de dar ubicaciones a todos los materiales
                        throw new Exception("SelectedSample is empty");
                    }

                    if (!currentCell.IsSelected) // Unselected
                    {
                        if (SelectedSelectableSample.Selected)
                            throw new WarningException("Sample is already located for selected pathogen");

                        currentCell.request_process_assay_id = pairedCell.request_process_assay_id = SelectedRequestProcessAssay.request_process_essay_id;
                        currentCell.composite_sample_id = pairedCell.composite_sample_id = SelectedSelectableSample.Item.composite_sample_id;
                        currentCell.request_id = pairedCell.request_id = SelectedRequestProcessAssay.request_id;
                        currentCell.sample_type_id = pairedCell.sample_type_id = SelectedRequestProcessAssay.sample_type_id;
                        currentCell.DataType = pairedCell.DataType = ReadingDataEntry.Entry;
                        currentCell.IsSelected = pairedCell.IsSelected = true;

                        //currentCell.Title = pairedCell.Title = CurrentSampleMaterial.Item.num_order.ToString();
                        currentCell.Title = pairedCell.Title = Utils.FormatSampleNameAsTitle(SelectedSelectableSample.Item.composite_sample_name, 1);
                        currentCell.Subtitle = pairedCell.Subtitle = Utils.FormatSampleNameAsTitle(SelectedSelectableSample.Item.composite_sample_name, 2);
                        currentCell.Color = pairedCell.Color = _requestColorPalette[SelectedRequestProcessAssay.request_id];
                        currentCell.SampleTypeIndicator = pairedCell.SampleTypeIndicator = Utils.ConvertToSampleTypeIndicator(SelectedRequestProcessAssay.sample_type_id);

                        //_cachePathogenSamples.Add(new SupportLocation { agent_id = pathogenId, composite_sample_id = CurrentSampleMaterial.Item.composite_sample_id });
                        if (updateCurrentSample)
                        {
                            SelectedSelectableSample.Selected = true;

                            //Select next available material
                            var iMaterial = SelectableSampleList.IndexOf(SelectedSelectableSample);
                            if (iMaterial <= SelectableSampleList.Count - 2)
                            {
                                SelectedSelectableSample = SelectableSampleList[++iMaterial];
                                SampleIndex = iMaterial + 1;
                                UpdateRequesProcessAssayLastSample(SelectedRequestProcessAssay.request_process_essay_id, SelectedSelectableSample.Item.composite_sample_id);
                            }
                        }
                    }
                }
                else
                {
                    string cellTitle = string.Empty;
                    string cellColor = "White";
                    ReadingDataEntry cellDataType;
                    switch (SelectedEntryType)
                    {
                        case ReadingDataEntry.Positive:
                            cellTitle = "P";
                            cellColor = _positiveBackgroundColor;
                            cellDataType = ReadingDataEntry.Positive;
                            break;
                        case ReadingDataEntry.Negative:
                            cellTitle = "N";
                            cellColor = _negativeBackgroundColor;
                            cellDataType = ReadingDataEntry.Negative;
                            break;
                        case ReadingDataEntry.Buffer:
                            cellTitle = "B";
                            cellColor = _bufferBackgroundColor;
                            cellDataType = ReadingDataEntry.Buffer;
                            break;
                        default:
                            throw new Exception("Entry type not supported");
                    }

                    if (!currentCell.IsSelected) // Unselected
                    {
                        currentCell.composite_sample_id = pairedCell.composite_sample_id = 0;
                        currentCell.request_id = pairedCell.request_id = 0;
                        currentCell.sample_type_id = pairedCell.sample_type_id = 0;
                        currentCell.DataType = pairedCell.DataType = cellDataType;
                        currentCell.IsSelected = pairedCell.IsSelected = true;

                        currentCell.Title = pairedCell.Title = cellTitle;
                        currentCell.Color = pairedCell.Color = cellColor;
                    }
                }
            }
        }
        public DelegateCommand SaveCommand { get; }
        private async void ExecuteSaveCommand()
        {
            try
            {
                IsBusy = true;

                var distributions = new List<SupportLocation>();

                
                //Init pending request process assays
                var requestProcessAssayIds = new List<int>();
                var tempRequestProcessAssayIds = LocationList.Where(x => x.request_process_assay_id > 0)
                        .Select(x => x.request_process_assay_id)
                        .Distinct()
                        .ToArray();
                requestProcessAssayIds.AddRange(tempRequestProcessAssayIds);
                var uniqueRequestProcessAssayIds = requestProcessAssayIds.Distinct();
                foreach (var requestProcessAssayId in uniqueRequestProcessAssayIds)
                {
                    //if (_cacheRequestProcessAssays.Keys.Contains(requestProcessAssayId) && _cacheRequestProcessAssays[requestProcessAssayId].status_name.Equals("pending"))
                    //{
                    //    var apiResponse = await _requestService.UpdateAssayStep(requestProcessAssayId, 0, "init");
                    //    if (apiResponse.Status != 200)
                    //    {
                    //        throw new Exception(apiResponse.Message);
                    //    }
                    //}
                }

                foreach (var pathogenId in _pathogenIds)
                {
                    var _supportList = _pathogenPlates[pathogenId]; //var _supportList = LocationList.ToList();
                    //for (int iSupport = 0; iSupport < _supportList.Count(); iSupport++)
                    //{
                    var allRequestProcessAssayIds = _supportList[0]//[iSupport]
                        .Where(x => x.request_process_assay_id > 0)
                        .Select(x => x.request_process_assay_id)
                        .Distinct();

                    foreach (var supportLocation in _supportList[0]/*[iSupport]*/)
                    {
                        if (supportLocation.IsSelected)
                        {
                            //Send only odd columns
                            int columnOrder = (supportLocation.Position - 1) / 8 + 1;
                            if (columnOrder % 2 != 0)
                            {
                                if (supportLocation.DataType == ReadingDataEntry.Entry)
                                {
                                    distributions.Add(new SupportLocation
                                    {
                                        event_id = _elisaEvent.event_id,
                                        agent_id = pathogenId/*pathogenItem.PathogenId*/,
                                        support_order_id = _plateOrder/*iSupport + 1*/,
                                        cell_position = supportLocation.Position,
                                        request_process_essay_id = supportLocation.request_process_assay_id,
                                        composite_sample_id = supportLocation.composite_sample_id,
                                        reading_data_type_id = (int)supportLocation.DataType,
                                    });
                                }
                                else
                                {
                                    foreach (var requestProcessAssayId in allRequestProcessAssayIds)
                                    {
                                        Dictionary<string, SampleMaterial> tokens = _TokenSampleCache[requestProcessAssayId];
                                        int compositeSampleId = tokens[supportLocation.DataType.ToString().ToLowerInvariant()].composite_sample_id;

                                        distributions.Add(new SupportLocation
                                        {
                                            event_id = _elisaEvent.event_id,
                                            agent_id = pathogenId/*pathogenItem.PathogenId*/,
                                            support_order_id = _plateOrder/*iSupport + 1*/,
                                            cell_position = supportLocation.Position,
                                            request_process_essay_id = requestProcessAssayId,
                                            composite_sample_id = compositeSampleId,
                                            reading_data_type_id = (int)supportLocation.DataType,
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            int columnOrder = (supportLocation.Position - 1) / 8 + 1;
                            if (columnOrder % 2 != 0)
                            {
                                //Check if cell was selected
                                var foundReadingData = _platesDistributions
                                //.Any(x => x.agent_id == pathogenItem.PathogenId && x.support_order_id == (iSupport + 1) && x.support_cell_position == supportLocation.Position);
                                .FirstOrDefault(x => x.agent_id == pathogenId && x.support_order_id == _plateOrder && x.support_cell_position == supportLocation.Position);
                                var wasSelected = foundReadingData != null;
                                if (wasSelected)
                                {
                                    distributions.Add(new SupportLocation
                                    {
                                        event_id = _elisaEvent.event_id,
                                        request_process_essay_id = foundReadingData.request_process_essay_id/*0*/,
                                        agent_id = pathogenId/*pathogenItem.PathogenId*/,
                                        composite_sample_id = null,
                                        reading_data_type_id = 0,
                                        support_order_id = _plateOrder/*iSupport + 1*/,
                                        cell_position = supportLocation.Position,
                                        to_delete = true
                                    });
                                }
                            }
                        }
                        //}
                    }
                }

                string jsonString = JsonConvert.SerializeObject(distributions, Formatting.Indented);
                await Xamarin.Essentials.Clipboard.SetTextAsync(jsonString);
                //await PageDialogService.DisplayAlertAsync("Json",jsonString,"OK");

                var result = await PageDialogService.DisplayAlertAsync("Message", "Are you sure continue?", "Yes", "No");
                if (result)
                {
                    var warningMessage = await _requestService.UpsertElisaDistribution(distributions);
                    IsBusy = false;

                    //TODO: Send notification to refresh plate thumbnail and reload modified/removed cells (reading_data)
                    EventAggregator.GetEvent<ElisaPlateDistributionSavedEvent>().Publish($"{string.Join(",",_pathogenIds)}|{_plateOrder}"); //pathogenIds|plateOrder

                    if(string.IsNullOrEmpty(warningMessage))
                        await PageDialogService.DisplayAlertAsync("Message", "Distribution has been saved successfully", "OK");
                    else
                        await PageDialogService.DisplayAlertAsync("Warning", "Distribution has been saved with some observations \n" + warningMessage, "OK");
                    var navigationResult = await NavigationService.GoBackAsync(new NavigationParameters
                    {
                        {"RefreshPlateThumbnail", true }
                    });
                }
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
        public override void Initialize(INavigationParameters parameters)
        {
            _elisaEvent = parameters.ContainsKey("ElisaEvent")
                ? parameters.GetValue<ElisaEvent>("ElisaEvent")
                : throw new Exception($"ElisaEvent is required");

            RequestProcessAssayList = parameters.ContainsKey("RequestProcessAssayList")
                ? parameters.GetValue<IEnumerable<SampleType>>("RequestProcessAssayList")
                : throw new Exception($"{nameof(RequestProcessAssayList)} is required");

            if (parameters.ContainsKey("PlatesDistributions"))
                _platesDistributions = parameters.GetValue<IEnumerable<SupportLocation>>("PlatesDistributions");

            if (parameters.ContainsKey("PathogenIds"))
            {
                _pathogenIds = parameters.GetValue<int[]>("PathogenIds");
                if(_pathogenIds.Length == 0)
                    throw new Exception("PathogenIds is empty");
                else if (_pathogenIds.Length == 1)
                {
                    _pathogenId = _pathogenIds[0];
                    IsMultiplePathogensMode = false;
                }
                else
                {
                    IsMultiplePathogensMode = true;
                }
                foreach (var pathogenId in _pathogenIds)
                {
                    _pathogenPlates.Add(pathogenId, new List<IEnumerable<PlateLocation>>());
                }
                _pathogenPlates.Add(0, new List<IEnumerable<PlateLocation>>());
            }
            else
                throw new Exception("PathogenIds is required");

            if (parameters.ContainsKey("PathogenNames"))
            {
                _pathogenNames = parameters.GetValue<string[]>("PathogenNames");
                if (_pathogenNames.Length == 0)
                    throw new Exception("PathogenNames is empty");
                else if (_pathogenNames.Length == 1)
                {
                    _pathogenName = _pathogenNames[0];
                }
            }
            else
                throw new Exception("PathogenNames is required");

            if (parameters.ContainsKey("RequestProcessAssayLastSamples"))
            {
                RequestProcessAssayLastSamples = parameters.GetValue<Dictionary<int, int>>("RequestProcessAssayLastSamples");
            }
            else
                throw new Exception("RequestProcessAssayLastSamples is required");

            _plateOrder = parameters.ContainsKey("PlateOrder")
                ? parameters.GetValue<int>("PlateOrder")
                : throw new Exception("PlateOrder is required");

            Title = $"Plate {_plateOrder} for {string.Join(", ",_pathogenNames)}";

            Dictionary<string, string> tempColorList = new Dictionary<string, string>();
            foreach (var requestProcess in _elisaEvent.request_process)
            {
                int lastDigit = 0;
                if (int.TryParse(requestProcess.request_code.Substring(requestProcess.request_code.Length - 1), out lastDigit))
                {
                    if (!_requestColorPalette.ContainsKey(requestProcess.request_id))
                        _requestColorPalette.Add(requestProcess.request_id, ColorPalette.Colors[lastDigit]);
                    if (!tempColorList.ContainsKey(requestProcess.request_code))
                        tempColorList.Add(requestProcess.request_code, ColorPalette.Colors[lastDigit]);
                }
                else
                    throw new Exception("Invalid request code");
            }
            ColorLegend = tempColorList;
        }
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                IsBusy = true;

                if (!IsLoaded)
                {
                    //await System.Threading.Tasks.Task.Delay(10);
                    if (_platesDistributions.Any())
                    {
                        //Load plate nro 1
                        if (!IsMultiplePathogensMode)
                        {
                            var plateOrder = _plateOrder;
                            var pathogenPlatesDistributions = _platesDistributions;

                            List<PlateLocation> tempLocationList = new List<PlateLocation>();
                            for (int iRow = 0; iRow < 8; iRow++)
                            {
                                for (int iCol = 0; iCol < 12; iCol++)
                                {
                                    var cellPosition = (iCol * 8) + iRow + 1;
                                    var cellPositionTitle = Utils.ConvertPositionToPositionTitle(iRow, iCol);
                                    var pathogenPlateCell = pathogenPlatesDistributions
                                        .FirstOrDefault(x => x.support_order_id == plateOrder && x.support_cell_position == cellPosition);

                                    if (pathogenPlateCell != null)
                                    {
                                        if (pathogenPlateCell.reading_data_type_id == (int)ReadingDataEntry.Entry)
                                        {
                                            tempLocationList.Add(new PlateLocation
                                            {
                                                Position = cellPosition,
                                                PositionTitle = cellPositionTitle,
                                                Color = _requestColorPalette[pathogenPlateCell.request_id],
                                                composite_sample_id = (int)pathogenPlateCell.composite_sample_id,
                                                request_id = pathogenPlateCell.request_id,
                                                request_process_assay_id = pathogenPlateCell.request_process_essay_id,
                                                IsSelected = true,
                                                DataType = (ReadingDataEntry)pathogenPlateCell.reading_data_type_id,
                                                //Title = pathogenPlateCell.composite_sample_order.ToString(),
                                                Title = Utils.FormatSampleNameAsTitle(pathogenPlateCell.composite_sample_name, 1),
                                                Subtitle = Utils.FormatSampleNameAsTitle(pathogenPlateCell.composite_sample_name, 2),
                                                SampleTypeIndicator = Utils.ConvertToSampleTypeIndicator(pathogenPlateCell.sample_type_id),
                                            });
                                        }
                                        else //Controles
                                        {
                                            string cellTitle = string.Empty;
                                            string cellColor = "White";
                                            switch (pathogenPlateCell.reading_data_type_id)
                                            {
                                                case (int)ReadingDataEntry.Positive:
                                                    cellTitle = "P";
                                                    cellColor = _positiveBackgroundColor;
                                                    break;
                                                case (int)ReadingDataEntry.Negative:
                                                    cellTitle = "N";
                                                    cellColor = _negativeBackgroundColor;
                                                    break;
                                                case (int)ReadingDataEntry.Buffer:
                                                    cellTitle = "B";
                                                    cellColor = _bufferBackgroundColor;
                                                    break;
                                                default:
                                                    throw new Exception("Entry type not supported");
                                            }

                                            tempLocationList.Add(new PlateLocation
                                            {
                                                Position = cellPosition,
                                                PositionTitle = cellPositionTitle,
                                                Color = cellColor,
                                                composite_sample_id = 0,
                                                request_id = 0,
                                                request_process_assay_id = 0,
                                                IsSelected = true,
                                                DataType = (ReadingDataEntry)pathogenPlateCell.reading_data_type_id,
                                                Title = cellTitle,
                                                SampleTypeIndicator = string.Empty,
                                            });
                                        }
                                    }
                                    else
                                    {
                                        tempLocationList.Add(new PlateLocation
                                        {
                                            Position = cellPosition,
                                            PositionTitle = cellPositionTitle,
                                            Color = "White",
                                        });
                                    }
                                }
                            }
                            _pathogenPlates[_pathogenId].Add(tempLocationList);
                            LocationList = tempLocationList;

                            //Check if materials are already placed
                            var placedPathogenSampleIds = _platesDistributions.Where(x => x.agent_id == _pathogenId).Select(x => x.composite_sample_id).Distinct();
                            foreach (var placedSampleId in placedPathogenSampleIds)
                            {
                                _cachePathogenSamples.Add(new SupportLocation
                                {
                                    agent_id = _pathogenId,
                                    composite_sample_id = placedSampleId,
                                });
                            }
                            //Load request process assays status
                            var requestProcessAssayIds = _platesDistributions.Select(x => x.request_process_essay_id).Distinct();
                            //foreach (var reqProAssayId in requestProcessAssayIds)
                            //{
                            //    var requestProcessAssay = await _requestService.GetRequestProcessAssay(reqProAssayId);

                            //    if (_cacheRequestProcessAssays.ContainsKey(requestProcessAssay.request_process_essay_id))
                            //        _cacheRequestProcessAssays[requestProcessAssay.request_process_essay_id] = requestProcessAssay;
                            //    else
                            //        _cacheRequestProcessAssays.Add(requestProcessAssay.request_process_essay_id, requestProcessAssay);
                            //}
                            //Load request process assays control samples
                            foreach (var reqProAssayId in requestProcessAssayIds)
                            {
                                var controlSamples = await _requestService.GetElisaControlSamplesAsync(reqProAssayId);
                                if (controlSamples.Count() != 3)
                                    throw new Exception("All control samples not found");
                                var controlSampleMap = new Dictionary<string, SampleMaterial>();
                                foreach (var controlSample in controlSamples)
                                {
                                    controlSampleMap.Add(controlSample.composite_sample_name.ToLowerInvariant(), controlSample);
                                }
                                _TokenSampleCache.Add(reqProAssayId, controlSampleMap);
                            }
                        }
                        else
                        {
                            //Check if materials are already placed
                            foreach (var pathogenId in _pathogenIds)
                            {
                                var placedPathogenSampleIds = _platesDistributions.Where(x => x.agent_id == pathogenId).Select(x => x.composite_sample_id).Distinct();
                                foreach (var placedSampleId in placedPathogenSampleIds)
                                {
                                    _cachePathogenSamples.Add(new SupportLocation
                                    {
                                        agent_id = pathogenId,
                                        composite_sample_id = placedSampleId,
                                    });
                                }
                            }

                            //Load request process assays status
                            var requestProcessAssayIds = _platesDistributions.Select(x => x.request_process_essay_id).Distinct();
                            //foreach (var reqProAssayId in requestProcessAssayIds)
                            //{
                            //    var requestProcessAssay = await _requestService.GetRequestProcessAssay(reqProAssayId);

                            //    if (_cacheRequestProcessAssays.ContainsKey(requestProcessAssay.request_process_essay_id))
                            //        _cacheRequestProcessAssays[requestProcessAssay.request_process_essay_id] = requestProcessAssay;
                            //    else
                            //        _cacheRequestProcessAssays.Add(requestProcessAssay.request_process_essay_id, requestProcessAssay);
                            //}
                            //Load request process assays control samples
                            foreach (var reqProAssayId in requestProcessAssayIds)
                            {
                                var controlSamples = await _requestService.GetElisaControlSamplesAsync(reqProAssayId);
                                if (controlSamples.Count() != 3)
                                    throw new Exception("All control samples not found");
                                var controlSampleMap = new Dictionary<string, SampleMaterial>();
                                foreach (var controlSample in controlSamples)
                                {
                                    controlSampleMap.Add(controlSample.composite_sample_name.ToLowerInvariant(), controlSample);
                                }
                                _TokenSampleCache.Add(reqProAssayId, controlSampleMap);
                            }

                            //Load plate distribution for all pathogens
                            foreach (var pathogenId in _pathogenIds)
                            {
                                var pathogenPlatesDistributions = _platesDistributions.Where(x => x.agent_id == pathogenId);
                                //Load plate distribution
                                List<PlateLocation> plateLocationList = new List<PlateLocation>();
                                for (int iRow = 0; iRow < 8; iRow++)
                                {
                                    for (int iCol = 0; iCol < 12; iCol++)
                                    {
                                        var cellPosition = (iCol * 8) + iRow + 1;
                                        var cellPositionTitle = Utils.ConvertPositionToPositionTitle(iRow, iCol);
                                        var pathogenPlateCell = pathogenPlatesDistributions
                                            .FirstOrDefault(x => x.support_order_id == _plateOrder && x.support_cell_position == cellPosition);

                                        if (pathogenPlateCell != null)
                                        {
                                            if (pathogenPlateCell.reading_data_type_id == (int)ReadingDataEntry.Entry)
                                            {
                                                plateLocationList.Add(new PlateLocation
                                                {
                                                    Position = cellPosition,
                                                    PositionTitle = cellPositionTitle,
                                                    Color = _requestColorPalette[pathogenPlateCell.request_id],
                                                    composite_sample_id = (int)pathogenPlateCell.composite_sample_id,
                                                    request_id = pathogenPlateCell.request_id,
                                                    request_process_assay_id = pathogenPlateCell.request_process_essay_id,
                                                    IsSelected = true,
                                                    DataType = (ReadingDataEntry)pathogenPlateCell.reading_data_type_id,
                                                    //Title = pathogenPlateCell.composite_sample_order.ToString(),
                                                    Title = Utils.FormatSampleNameAsTitle(pathogenPlateCell.composite_sample_name, 1),
                                                    Subtitle = Utils.FormatSampleNameAsTitle(pathogenPlateCell.composite_sample_name, 2),
                                                    SampleTypeIndicator = Utils.ConvertToSampleTypeIndicator(pathogenPlateCell.sample_type_id),
                                                });
                                            }
                                            else //Controles
                                            {
                                                string cellTitle = string.Empty;
                                                string cellColor = "White";
                                                switch (pathogenPlateCell.reading_data_type_id)
                                                {
                                                    case (int)ReadingDataEntry.Positive:
                                                        cellTitle = "P";
                                                        cellColor = _positiveBackgroundColor;
                                                        break;
                                                    case (int)ReadingDataEntry.Negative:
                                                        cellTitle = "N";
                                                        cellColor = _negativeBackgroundColor;
                                                        break;
                                                    case (int)ReadingDataEntry.Buffer:
                                                        cellTitle = "B";
                                                        cellColor = _bufferBackgroundColor;
                                                        break;
                                                    default:
                                                        throw new Exception("Entry type not supported");
                                                }

                                                plateLocationList.Add(new PlateLocation
                                                {
                                                    Position = cellPosition,
                                                    PositionTitle = cellPositionTitle,
                                                    Color = cellColor,
                                                    composite_sample_id = 0,
                                                    request_id = 0,
                                                    request_process_assay_id = 0,
                                                    IsSelected = true,
                                                    DataType = (ReadingDataEntry)pathogenPlateCell.reading_data_type_id,
                                                    Title = cellTitle,
                                                    SampleTypeIndicator = string.Empty,
                                                });
                                            }
                                        }
                                        else
                                        {
                                            plateLocationList.Add(new PlateLocation
                                            {
                                                Position = cellPosition,
                                                PositionTitle = cellPositionTitle,
                                                Color = "White",
                                            });
                                        }
                                    }
                                }
                                _pathogenPlates[pathogenId].Add(plateLocationList);
                                //_pathogenPlates.Add(pathogenId, new List<IEnumerable<PlateLocation>> { plateLocationList });
                            }
                            //Virtual pathogen to identify all
                            var allPathogenId = 0;
                            List<PlateLocation> allPathogenLocationList = new List<PlateLocation>();
                            for (int iRow = 0; iRow < 8; iRow++)
                            {
                                for (int iCol = 0; iCol < 12; iCol++)
                                {
                                    var cellPosition = (iCol * 8) + iRow + 1;
                                    var cellPositionTitle = Utils.ConvertPositionToPositionTitle(iRow, iCol);

                                    //For now only One plate exists so use FirstOrDefault
                                    var selectedCellsByPositionForAllPathogens = new List<PlateLocation>();
                                    foreach (var pathogenId in _pathogenIds)
                                    {
                                        var foundCell = _pathogenPlates[pathogenId].FirstOrDefault().FirstOrDefault(x => x.Position == cellPosition && x.IsSelected);
                                        if (foundCell != null)
                                            selectedCellsByPositionForAllPathogens.Add(foundCell);
                                    }
                                    if (selectedCellsByPositionForAllPathogens.Any())
                                    {
                                        var firstSelectedCellByPosition = selectedCellsByPositionForAllPathogens.FirstOrDefault();
                                        if (selectedCellsByPositionForAllPathogens.Count != _pathogenIds.Length) // ReadOnly
                                        {
                                            allPathogenLocationList.Add(new PlateLocation
                                            {
                                                Position = cellPosition,
                                                PositionTitle = cellPositionTitle,
                                                Color = "Gray",
                                            });
                                        }
                                        else
                                        {
                                            if (selectedCellsByPositionForAllPathogens.Any(x => x.composite_sample_id != firstSelectedCellByPosition.composite_sample_id)) //ReadOnly
                                            {
                                                allPathogenLocationList.Add(new PlateLocation
                                                {
                                                    Position = cellPosition,
                                                    PositionTitle = cellPositionTitle,
                                                    Color = "DarkGray",
                                                });
                                            }
                                            else //Sample composite_sample for all pathogens
                                            {
                                                //When IsSelected= true and composite_sample_id = 0 then is ControlCell
                                                if (firstSelectedCellByPosition.composite_sample_id == 0)
                                                {
                                                    if (selectedCellsByPositionForAllPathogens.Any(x => x.DataType != firstSelectedCellByPosition.DataType)) //ReadOnly
                                                    {
                                                        allPathogenLocationList.Add(new PlateLocation
                                                        {
                                                            Position = cellPosition,
                                                            PositionTitle = cellPositionTitle,
                                                            Color = "DarkGray",
                                                        });
                                                    }
                                                    else
                                                    {
                                                        string cellTitle;
                                                        string cellColor;
                                                        switch (firstSelectedCellByPosition.DataType)
                                                        {
                                                            case ReadingDataEntry.Positive:
                                                                cellTitle = "P";
                                                                cellColor = _positiveBackgroundColor;
                                                                break;
                                                            case ReadingDataEntry.Negative:
                                                                cellTitle = "N";
                                                                cellColor = _negativeBackgroundColor;
                                                                break;
                                                            case ReadingDataEntry.Buffer:
                                                                cellTitle = "B";
                                                                cellColor = _bufferBackgroundColor;
                                                                break;
                                                            default:
                                                                throw new Exception("Cell type not supported");
                                                        }

                                                        allPathogenLocationList.Add(new PlateLocation
                                                        {
                                                            Position = cellPosition,
                                                            PositionTitle = cellPositionTitle,
                                                            Color = cellColor,
                                                            composite_sample_id = 0,
                                                            request_id = 0,
                                                            request_process_assay_id = 0,
                                                            IsSelected = true,
                                                            DataType = firstSelectedCellByPosition.DataType,
                                                            Title = cellTitle,
                                                            SampleTypeIndicator = string.Empty,
                                                        });
                                                    }
                                                }
                                                else
                                                {
                                                    allPathogenLocationList.Add(new PlateLocation
                                                    {
                                                        Position = cellPosition,
                                                        PositionTitle = cellPositionTitle,
                                                        Color = _requestColorPalette[firstSelectedCellByPosition.request_id],
                                                        composite_sample_id = firstSelectedCellByPosition.composite_sample_id,
                                                        request_id = firstSelectedCellByPosition.request_id,
                                                        request_process_assay_id = firstSelectedCellByPosition.request_process_assay_id,
                                                        IsSelected = true,
                                                        DataType = firstSelectedCellByPosition.DataType,
                                                        Title = firstSelectedCellByPosition.Title,
                                                        Subtitle = firstSelectedCellByPosition.Subtitle,
                                                        SampleTypeIndicator = firstSelectedCellByPosition.SampleTypeIndicator,
                                                    });
                                                }
                                            }
                                        }
                                    }
                                    else //Empty cell for all pathogens
                                    {
                                        allPathogenLocationList.Add(new PlateLocation
                                        {
                                            Position = cellPosition,
                                            PositionTitle = cellPositionTitle,
                                            Color = "white",
                                        });
                                    }                                    
                                }
                            }
                            
                            _pathogenPlates[0].Add(allPathogenLocationList);
                            //LocationList = _pathogenPlates[allPathogenId].FirstOrDefault();
                            LocationList = allPathogenLocationList;
                        }

                        
                    }
                    else
                    {
                        foreach (var pathogenId in _pathogenIds)
                        {
                            List<PlateLocation> tempLocationList = new List<PlateLocation>();
                            for (int y = 0; y < 8; y++)
                            {
                                for (int x = 0; x < 12; x++)
                                {
                                    tempLocationList.Add(new PlateLocation
                                    {
                                        Position = (x * 8) + y + 1,
                                        PositionTitle = Utils.ConvertPositionToPositionTitle(y, x),
                                        Color = "White",
                                    });
                                }
                            }
                            _pathogenPlates[pathogenId].Add(tempLocationList);
                        }
                        if (IsMultiplePathogensMode)
                        {
                            List<PlateLocation> allPathogenLocationList = new List<PlateLocation>();
                            for (int y = 0; y < 8; y++)
                            {
                                for (int x = 0; x < 12; x++)
                                {
                                    allPathogenLocationList.Add(new PlateLocation
                                    {
                                        Position = (x * 8) + y + 1,
                                        PositionTitle = Utils.ConvertPositionToPositionTitle(y, x),
                                        Color = "White",
                                    });
                                }
                            }
                            _pathogenPlates[0].Add(allPathogenLocationList);
                            LocationList = _pathogenPlates[0].First();
                        }
                        else
                        {
                            LocationList = _pathogenPlates[_pathogenIds.First()].First();
                        }
                    }
                    IsLoaded = true;
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayErrorAlertAsync(ex);
                IsLoaded = false;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
