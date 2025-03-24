using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Prism.Services.Dialogs;
using SeedHealthApp.Extensions;
using SeedHealthApp.Helpers;
using SeedHealthApp.Models;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace SeedHealthApp.ViewModels
{
    public class ElisaDistributionPageViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;
        private readonly IDialogService _dialogService;
        private Dictionary<int, string> _requestColorPalette = new Dictionary<int, string>();
        private ElisaDistribution _elisaDistribution = new ElisaDistribution();
        private List<IEnumerable<PlateLocation>> _supportList = new List<IEnumerable<PlateLocation>>();
        //private Dictionary<string, Dictionary<string, SampleMaterial>> _tokens = new Dictionary<string, Dictionary<string, SampleMaterial>>();
        private Dictionary<int, Dictionary<string, SampleMaterial>> _TokenSampleCache = new Dictionary<int, Dictionary<string, SampleMaterial>>();
        private Dictionary<int, SampleType> _cacheRequestProcessAssays = new Dictionary<int, SampleType>();
        private ElisaEvent _elisaEvent;
        private Dictionary<int, List<IEnumerable<PlateLocation>>> _pathogenPlates = new Dictionary<int, List<IEnumerable<PlateLocation>>>();
        private bool _isLoaded;
        private IEnumerable<SupportLocation> _platesDistributions;
        private List<SupportLocation> _cachePathogenSamples = new List<SupportLocation>();
        private readonly string _positiveBackgroundColor = "#ffc4d6";
        private readonly string _negativeBackgroundColor = "#d0f4de";
        private readonly string _bufferBackgroundColor = "#adb5bd";

        private int _supportCount;
        public int SupportCount
        {
            get { return _supportCount; }
            set { SetProperty(ref _supportCount, value); }
        }
        private int _supportOrder;
        public int SupportOrder
        {
            get { return _supportOrder; }
            set { SetProperty(ref _supportOrder, value); }
        }

        private Dictionary<string, string> _colorList;
        public Dictionary<string, string> ColorList
        {
            get { return _colorList; }
            set { SetProperty(ref _colorList, value); }
        }

        private List<SelectableModel<SampleMaterial>> _materialList;
        public List<SelectableModel<SampleMaterial>> MaterialList
        {
            get { return _materialList; }
            set { SetProperty(ref _materialList, value); }
        }

        private IEnumerable<PlateLocation> _locationList;
        public IEnumerable<PlateLocation> LocationList
        {
            get { return _locationList; }
            set { SetProperty(ref _locationList, value); }
        }
        private IEnumerable<RequestLookup> _selectedRequestList;
        public IEnumerable<RequestLookup> SelectedRequestList
        {
            get { return _selectedRequestList; }
            set { SetProperty(ref _selectedRequestList, value); }
        }
        private RequestLookup _selectedRequest;
        public RequestLookup SelectedRequest
        {
            get { return _selectedRequest; }
            set { SetProperty(ref _selectedRequest, value); }
        }
        private SelectableModel<SampleMaterial> _currentSampleMaterial;
        public SelectableModel<SampleMaterial> CurrentSampleMaterial
        {
            get { return _currentSampleMaterial; }
            set { SetProperty(ref _currentSampleMaterial, value); }
        }
        private IEnumerable<SampleType> _sampleTypeList;
        public IEnumerable<SampleType> SampleTypeList
        {
            get { return _sampleTypeList; }
            set { SetProperty(ref _sampleTypeList, value); }
        }
        private SampleType _selectedSampleType;
        public SampleType SelectedSampleType
        {
            get { return _selectedSampleType; }
            set { SetProperty(ref _selectedSampleType, value); }
        }
        private ReadingDataEntry _selectedEntryType;
        public ReadingDataEntry SelectedEntryType
        {
            get { return _selectedEntryType; }
            set { SetProperty(ref _selectedEntryType, value); }
        }
        private IEnumerable<PathogenItem> _pathogenList;
        public IEnumerable<PathogenItem> PathogenList
        {
            get { return _pathogenList; }
            set { SetProperty(ref _pathogenList, value); }
        }
        private PathogenItem _selectedPathogen;
        public PathogenItem SelectedPathogen
        {
            get { return _selectedPathogen; }
            set { SetProperty(ref _selectedPathogen, value); }
        }
        private bool _allPathogensIsChecked;
        public bool AllPathogensIsChecked
        {
            get { return _allPathogensIsChecked; }
            set { SetProperty(ref _allPathogensIsChecked, value); }
        }
        private bool _removingCells;
        public bool RemovingCells
        {
            get { return _removingCells; }
            set { SetProperty(ref _removingCells, value); }
        }
        private int _sampleIndex;
        public int SampleIndex
        {
            get { return _sampleIndex; }
            set { SetProperty(ref _sampleIndex, value); }
        }
        public ElisaDistributionPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IRequestService requestService, IDialogService dialogService, IEventAggregator eventAggregator) 
            : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _requestService = requestService;
            _dialogService = dialogService;

            Title = "Set distribution";
            Username = SettingsService.Username;

            PreviusCommand = new DelegateCommand(OnPreviusCommand).ObservesCanExecute(() => IsIdle);
            NextCommand = new DelegateCommand(OnNextCommand).ObservesCanExecute(() => IsIdle);
            PickCellCommand = new DelegateCommand<object>(OnPickCellCommand).ObservesCanExecute(() => IsIdle);
            LoadSampleMaterialsCommand = new DelegateCommand(OnLoadSampleMaterialsCommand).ObservesCanExecute(() => IsIdle);
            RefreshSampleTypeListCommand = new DelegateCommand(OnRefreshSampleTypeListCommand).ObservesCanExecute(() => IsIdle);
            SaveDistributionCommand = new DelegateCommand(OnSaveDistributionCommand).ObservesCanExecute(() => IsIdle);
            PathogenChangedCommand = new DelegateCommand(ExecutePathogenChangedCommand).ObservesCanExecute(() => IsIdle);
            PreviousSampleCommand = new DelegateCommand(ExecutePreviousSampleCommand).ObservesCanExecute(() => IsIdle);
            NextSampleCommand = new DelegateCommand(ExecuteNextSampleCommand).ObservesCanExecute(() => IsIdle);

            GoBackCommand = new DelegateCommand(OnGoBackCommand).ObservesCanExecute(() => IsIdle);
            RemoveSupport = new DelegateCommand(OnRemoveSupport).ObservesCanExecute(() => IsIdle);

            SelectedEntryType = ReadingDataEntry.Entry;
            MaterialList = new List<SelectableModel<SampleMaterial>>();
            _platesDistributions = Enumerable.Empty<SupportLocation>();
            AllPathogensIsChecked = true;
        }
        private DelegateCommand _showDebugInfoCommand;
        public DelegateCommand ShowDebugInfoCommand =>
            _showDebugInfoCommand ?? (_showDebugInfoCommand = new DelegateCommand(ExecuteShowDebugInfoCommand));

        private async void ExecuteShowDebugInfoCommand()
        {
            try
            {
                IsBusy = true;

                string message = "Selected Request" + Environment.NewLine +
                    Newtonsoft.Json.JsonConvert.SerializeObject(SelectedRequest, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Environment.NewLine + "SelectedRequestList" + Environment.NewLine +
                    Newtonsoft.Json.JsonConvert.SerializeObject(SelectedRequestList, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Environment.NewLine + "Selected Sample Type" + Environment.NewLine + 
                    Newtonsoft.Json.JsonConvert.SerializeObject(SelectedSampleType, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Environment.NewLine + "Current Sample" + Environment.NewLine +
                    Newtonsoft.Json.JsonConvert.SerializeObject(CurrentSampleMaterial, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Environment.NewLine + "Cache request process assay" + Environment.NewLine + Environment.NewLine +
                    Newtonsoft.Json.JsonConvert.SerializeObject(_cacheRequestProcessAssays, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Environment.NewLine + "Selected Pathogen" + Environment.NewLine +
                    Newtonsoft.Json.JsonConvert.SerializeObject(_selectedPathogen, Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Environment.NewLine + "Selected Plate" + Environment.NewLine +
                    Newtonsoft.Json.JsonConvert.SerializeObject(_supportOrder, Newtonsoft.Json.Formatting.Indented);
                //message += Environment.NewLine + Environment.NewLine + "Pathogen plate locations" + Environment.NewLine +
                //    Newtonsoft.Json.JsonConvert.SerializeObject(_pathogenPlates[_selectedPathogen.PathogenId][SupportOrder - 1], Newtonsoft.Json.Formatting.Indented);
                message += Environment.NewLine + Environment.NewLine + "Location List" + Environment.NewLine +
                    Newtonsoft.Json.JsonConvert.SerializeObject(LocationList, Newtonsoft.Json.Formatting.Indented);
                await PageDialogService.DisplayAlertAsync("", message, "OK");
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
        public DelegateCommand PreviusCommand { get; }
        private async void OnPreviusCommand()
        {
            try
            {
                if(SupportOrder > 1)
                {
                    IsBusy = true;
                    SupportOrder--;
                    LocationList = _pathogenPlates[_selectedPathogen.PathogenId][SupportOrder - 1];
                    IsBusy = false;
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand NextCommand { get; }
        private async void OnNextCommand()
        {
            try
            {
                IsBusy = true;
                if (SupportOrder == SupportCount)
                {
                    foreach (var pathogenId in _pathogenPlates.Keys)
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
                    SupportCount++;
                }

                SupportOrder++;
                LocationList = _pathogenPlates[_selectedPathogen.PathogenId][SupportOrder - 1];
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand RefreshSampleTypeListCommand { get; }
        private async void OnRefreshSampleTypeListCommand()
        {
            try
            {
                SelectedSampleType = null;
                SampleTypeList = null;

                if (SelectedRequest != null)
                {
                    IsBusy = true;

                    var sampleApiResponse = await _requestService.GetElisaSampleTypesByAgents(SelectedRequest.request_id, SelectedRequest.process_id, _elisaDistribution.agent_ids);
                    if (sampleApiResponse.Status == 200)
                    {
                        if (sampleApiResponse.Data != null && !sampleApiResponse.Data.Any())
                        {
                            SampleTypeList = new List<SampleType>();
                            CurrentSampleMaterial = null;
                            throw new Exception("Sample types available list is empty");
                        }

                        foreach (SampleType requestProcessAssay in sampleApiResponse.Data)
                        {
                            if (_cacheRequestProcessAssays.ContainsKey(requestProcessAssay.request_process_essay_id))
                                _cacheRequestProcessAssays[requestProcessAssay.request_process_essay_id] = requestProcessAssay;
                            else
                                _cacheRequestProcessAssays.Add(requestProcessAssay.request_process_essay_id, requestProcessAssay);
                        }

                        SampleTypeList = sampleApiResponse.Data;
                        CurrentSampleMaterial = null;
                    }
                    else
                        throw new Exception(sampleApiResponse.Message);

                    //var sampleApiResponse = await _requestService.GetRequestProcessAssays(_selectedRequest.request_id, (int)AssayMobileEnum.Elisa);

                    //foreach (SampleType requestProcessAssay in sampleApiResponse)
                    //{
                    //    if (_cacheRequestProcessAssays.ContainsKey(requestProcessAssay.request_process_essay_id))
                    //        _cacheRequestProcessAssays[requestProcessAssay.request_process_essay_id] = requestProcessAssay;
                    //    else
                    //        _cacheRequestProcessAssays.Add(requestProcessAssay.request_process_essay_id, requestProcessAssay);
                    //}

                    //SampleTypeList = sampleApiResponse;

                    IsBusy = false;
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand LoadSampleMaterialsCommand { get; }
        private async void OnLoadSampleMaterialsCommand()
        {
            try
            {
                if(SelectedRequest != null && SelectedSampleType != null)
                {
                    IsBusy = true;

                    CurrentSampleMaterial = null;

                    var materials = await _requestService.GetRequestProcessAssaySamples(SelectedSampleType.request_process_essay_id);
                    //var materials = await _requestService.GetElisaSamplesAsync(SelectedSampleType.request_process_essay_id);

                    var tempMaterialList = new List<SelectableModel<SampleMaterial>>();
                    foreach (var material in materials)
                    {
                        var sampleIsPlaced = _cachePathogenSamples.Any(x => x.agent_id == _selectedPathogen.PathogenId && x.composite_sample_id == material.composite_sample_id);
                        tempMaterialList.Add(new SelectableModel<SampleMaterial>
                        {
                            Selected = sampleIsPlaced,
                            Item = material
                        });
                    }
                    if (!tempMaterialList.Any())
                        throw new Exception("Empty list of materials");

                    //Update selected materials from SupportList
                    foreach (var support in _supportList)
                    {
                        var partialSelectedMaterialIds = support
                            .Where(x => x.IsSelected && x.DataType == ReadingDataEntry.Entry
                                && x.request_id == SelectedRequest.request_id && x.sample_type_id == SelectedSampleType.sample_type_id)
                            .Select(x => x.composite_sample_id)
                            .Distinct()
                            .ToList();

                        foreach (var materialId in partialSelectedMaterialIds)
                        {
                            var selectedMaterial = tempMaterialList.FirstOrDefault(x => x.Item.composite_sample_id == materialId);
                            if (selectedMaterial != null)
                                selectedMaterial.Selected = true;
                        }
                    }

                    MaterialList = tempMaterialList;

                    if (!_TokenSampleCache.ContainsKey(SelectedSampleType.request_process_essay_id))
                    {
                        var controlSamples = await _requestService.GetElisaControlSamplesAsync(SelectedSampleType.request_process_essay_id);
                        if (controlSamples.Count() != 3)
                            throw new Exception("All control samples not found");
                        var controlSampleMap = new Dictionary<string, SampleMaterial>();
                        foreach (var controlSample in controlSamples)
                        {
                            controlSampleMap.Add(controlSample.composite_sample_name.ToLowerInvariant(), controlSample);
                        }
                        _TokenSampleCache.Add(SelectedSampleType.request_process_essay_id, controlSampleMap);
                    }

                    //Select next available material
                    SampleIndex = 0;
                    foreach (var sample in MaterialList)
                    {
                        SampleIndex++;
                        if (!sample.Selected)
                        {
                            CurrentSampleMaterial = sample;
                            break;
                        }
                    }
                    if (CurrentSampleMaterial == null)
                    {
                        SampleIndex = MaterialList.Any() ? 1 : 0;
                        CurrentSampleMaterial = MaterialList.FirstOrDefault();
                    }

                    //var nextItem = MaterialList.FirstOrDefault(x => x.Selected == false);
                    //CurrentSampleMaterial = nextItem ?? MaterialList.FirstOrDefault();

                    IsBusy = false;
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                MaterialList = null;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand<object> PickCellCommand { get; }
        private async void OnPickCellCommand(object cellPosition)
        {
            try
            {
                if (!RemovingCells && SelectedEntryType == ReadingDataEntry.Entry)
                {
                    if (SelectedRequest == null)
                        throw new Exception("Select request");
                    if (SelectedSampleType == null)
                        throw new Exception("Select sample type");
                }
                
                var currentCell = cellPosition as PlateLocation;
                PlateLocation pairedCell;
                // Get paired cell based on column order
                int columnOrder = (currentCell.Position - 1) / 8 + 1;
                if (columnOrder % 2 == 0)
                {// Column order is even
                    pairedCell = LocationList.First(x => x.Position == currentCell.Position - 8);
                }
                else
                {
                    pairedCell = LocationList.First(x => x.Position == currentCell.Position + 8);
                }

                //if (_selectedPathogen.PathogenId == 0) //All
                if(AllPathogensIsChecked)
                {
                    //Check if sample is already located in other pathogens plates
                    if (!RemovingCells && SelectedEntryType == ReadingDataEntry.Entry)
                    {
                        var isPlacedInAnyPathogen = _cachePathogenSamples.Any(x => x.composite_sample_id == CurrentSampleMaterial.Item.composite_sample_id && x.agent_id != _selectedPathogen.PathogenId);
                        if (isPlacedInAnyPathogen)
                            throw new Exception("Can't set current sample location for all pathogens because the sample is already placed in other pathogen plate");
                    }
                    
                    foreach (var pathogenItem in PathogenList.Where(x=> x.PathogenId > 0 && x.PathogenId != _selectedPathogen.PathogenId))
                    {
                        var pathogenCurrentCell = _pathogenPlates[pathogenItem.PathogenId][_supportOrder - 1].First(x => x.Position == currentCell.Position);
                        var pathogenPairedCell = _pathogenPlates[pathogenItem.PathogenId][_supportOrder - 1].First(x => x.Position == pairedCell.Position);
                        HandlePick(pathogenCurrentCell, pathogenPairedCell, false, pathogenItem.PathogenId);
                    }
                    HandlePick(currentCell, pairedCell, true, _selectedPathogen.PathogenId);
                }
                else
                {
                    HandlePick(currentCell, pairedCell, true, _selectedPathogen.PathogenId);
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        private bool CheckIfCurrentSampleIsPlaced()
        {
            var sampleIsPlaced = false;
            sampleIsPlaced = _cachePathogenSamples.Any(x => x.agent_id == _selectedPathogen.PathogenId && x.composite_sample_id == CurrentSampleMaterial.Item.composite_sample_id);
            /*
            for (int iPlate = 0; iPlate < SupportCount; iPlate++)
            {
                sampleIsPlaced = _pathogenPlates[_selectedPathogen.PathogenId][_supportOrder - 1].Any(x => x.composite_sample_id == CurrentSampleMaterial.Item.composite_sample_id);
                if (sampleIsPlaced)
                {
                    break;
                }
            }
            */
            CurrentSampleMaterial.Selected = sampleIsPlaced;
            return sampleIsPlaced;
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
                        if (updateCurrentSample)
                        {
                            //Unselect material
                            var currentCellMaterial = MaterialList.FirstOrDefault(x => x.Item.composite_sample_id.Equals(currentCell.composite_sample_id));
                            if (currentCellMaterial != null)
                            {
                                currentCellMaterial.Selected = false;
                            }
                            //Select next available material
                            //var nextItem = MaterialList.FirstOrDefault(x => x.Selected == false);
                            //CurrentSampleMaterial = nextItem ?? MaterialList.FirstOrDefault();
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
                    //Check if current material is empty and CellPosition is unselected
                    if (!currentCell.IsSelected && CurrentSampleMaterial == null)
                    {
                        //Lista de materiales esta vacia o se terminaron de dar ubicaciones a todos los materiales
                        throw new Exception("All samples are located");
                    }

                    if (!currentCell.IsSelected) // Unselected
                    {
                        if (CurrentSampleMaterial.Selected)
                            throw new Exception("Sample is already placed for selected pathogen");

                        currentCell.request_process_assay_id = pairedCell.request_process_assay_id = SelectedSampleType.request_process_essay_id;
                        currentCell.composite_sample_id = pairedCell.composite_sample_id = CurrentSampleMaterial.Item.composite_sample_id;
                        currentCell.request_id = pairedCell.request_id = SelectedRequest.request_id;
                        currentCell.sample_type_id = pairedCell.sample_type_id = SelectedSampleType.sample_type_id;
                        currentCell.DataType = pairedCell.DataType = ReadingDataEntry.Entry;
                        currentCell.IsSelected = pairedCell.IsSelected = true;

                        //currentCell.Title = pairedCell.Title = CurrentSampleMaterial.Item.num_order.ToString();
                        currentCell.Title = pairedCell.Title = Utils.FormatSampleNameAsTitle(CurrentSampleMaterial.Item.composite_sample_name, 1);
                        currentCell.Subtitle = pairedCell.Subtitle = Utils.FormatSampleNameAsTitle(CurrentSampleMaterial.Item.composite_sample_name, 2);
                        currentCell.Color = pairedCell.Color = _requestColorPalette[SelectedRequest.request_id];
                        currentCell.SampleTypeIndicator = pairedCell.SampleTypeIndicator = Utils.ConvertToSampleTypeIndicator(SelectedSampleType.sample_type_id);

                        _cachePathogenSamples.Add(new SupportLocation { agent_id = pathogenId, composite_sample_id = CurrentSampleMaterial.Item.composite_sample_id });
                        if (updateCurrentSample)
                        {
                            CurrentSampleMaterial.Selected = true;

                            //Select next available material
                            var iMaterial = MaterialList.IndexOf(CurrentSampleMaterial);
                            if (iMaterial <= MaterialList.Count - 2)
                            {
                                CurrentSampleMaterial = MaterialList[++iMaterial];
                                SampleIndex = iMaterial + 1;
                            }
                            //var nextItem = MaterialList.FirstOrDefault(x => x.Selected == false);
                            //CurrentSampleMaterial = nextItem ?? MaterialList.FirstOrDefault();
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

        public DelegateCommand SaveDistributionCommand { get; }
        private async void OnSaveDistributionCommand()
        {
            try
            {
                IsBusy = true;

                var requestProcessAssayIds = new List<int>();
                foreach (var pathogenItem in _pathogenList.Where(x => x.PathogenId > 0))
                {
                    _supportList = _pathogenPlates[pathogenItem.PathogenId];
                    foreach (IEnumerable<PlateLocation> locations in _supportList)
                    {
                        var ids = locations
                            .Where(x => x.request_process_assay_id > 0)
                            .Select(x => x.request_process_assay_id)
                            .Distinct()
                            .ToArray();
                        requestProcessAssayIds.AddRange(ids);
                    }
                }
                var uniqueRequestProcessAssayIds = requestProcessAssayIds.Distinct();
                foreach (var requestProcessAssayId in uniqueRequestProcessAssayIds)
                {
                    if (_cacheRequestProcessAssays.Keys.Contains(requestProcessAssayId) && _cacheRequestProcessAssays[requestProcessAssayId].status_name.Equals("pending"))
                    {
                        var apiResponse = await _requestService.UpdateAssayStep(requestProcessAssayId, 0, "init");
                        if (apiResponse.Status != 200)
                        {
                            throw new Exception(apiResponse.Message);
                        }
                    }
                }

                var distributions = new List<SupportLocation>();
                foreach (var pathogenItem in _pathogenList.Where(x=>x.PathogenId > 0))
                {
                    _supportList = _pathogenPlates[pathogenItem.PathogenId];
                    for (int iSupport = 0; iSupport < _supportList.Count(); iSupport++)
                    {
                        var tokensMaterialsKeys = _supportList[iSupport]
                            .Where(x => x.request_id > 0)
                            .Select(x => $"{x.request_id}+{x.sample_type_id}")
                            .Distinct();

                        var allRequestProcessAssayIds = _supportList[iSupport]
                            .Where(x => x.request_process_assay_id > 0)
                            .Select(x => x.request_process_assay_id)
                            .Distinct();

                        foreach (var supportLocation in _supportList[iSupport])
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
                                            event_id =  _elisaEvent.event_id,
                                            request_process_essay_id = supportLocation.request_process_assay_id,
                                            agent_id = pathogenItem.PathogenId,
                                            composite_sample_id = supportLocation.composite_sample_id,
                                            reading_data_type_id = (int)supportLocation.DataType,
                                            support_order_id = iSupport + 1,
                                            cell_position = supportLocation.Position,

                                            //request_id = supportLocation.request_id,
                                            //sample_type_id = supportLocation.sample_type_id,
                                            //data_type = (int)supportLocation.DataType,
                                            //support_order = iSupport + 1,
                                        });
                                    }
                                    else
                                    {
                                        foreach (var requestProcessAssayId in allRequestProcessAssayIds)
                                        {
                                            //if (!_TokenSampleCache.ContainsKey(requestProcessAssayId))
                                            //{
                                            //    //Get token samples by requesProcessAssayId
                                            //    var reqProAssay = await _requestService.GetRequestProcessAssay(requestProcessAssayId);
                                            //    var samples = await _requestService.GetRequestProcessAssaySamples(
                                            //        reqProAssay.request_id,
                                            //        reqProAssay.process_id,
                                            //        5,
                                            //        reqProAssay.sample_type_id);
                                            //    _TokenSampleCache.Add(SelectedSampleType.request_process_essay_id, tokenList);
                                            //}

                                            Dictionary<string, SampleMaterial> tokens = _TokenSampleCache[requestProcessAssayId];
                                            int compositeSampleId = tokens[supportLocation.DataType.ToString().ToLowerInvariant()].composite_sample_id;

                                            distributions.Add(new SupportLocation
                                            {
                                                event_id = _elisaEvent.event_id,
                                                request_process_essay_id = requestProcessAssayId,
                                                agent_id = pathogenItem.PathogenId,
                                                composite_sample_id = compositeSampleId,
                                                reading_data_type_id = (int)supportLocation.DataType,
                                                support_order_id = iSupport + 1,
                                                cell_position = supportLocation.Position,
                                            });
                                        }
                                    }
                                }
                            }
                            else
                            {
                                int columnOrder = (supportLocation.Position - 1) / 8 + 1;
                                if(columnOrder % 2 != 0)
                                {
                                    //Check if cell was selected
                                    var wasSelected = _platesDistributions.Any(x => x.agent_id == pathogenItem.PathogenId && x.support_order_id == (iSupport + 1) && x.support_cell_position == supportLocation.Position);
                                    if (wasSelected)
                                    {
                                        distributions.Add(new SupportLocation
                                        {
                                            event_id = _elisaEvent.event_id,
                                            request_process_essay_id = 0,
                                            agent_id = pathogenItem.PathogenId,
                                            composite_sample_id = null,
                                            reading_data_type_id = 0,
                                            support_order_id = iSupport + 1,
                                            cell_position = supportLocation.Position,
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                

                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(distributions, Newtonsoft.Json.Formatting.Indented);
                await Xamarin.Essentials.Clipboard.SetTextAsync(jsonString);
                //await PageDialogService.DisplayAlertAsync("Json",jsonString,"OK");

                var result = await PageDialogService.DisplayAlertAsync("Message", "Are you sure continue?", "Yes", "No");
                if (result)
                {
                    await _requestService.UpsertElisaDistribution(distributions);
                    IsBusy = false;
                    
                    await PageDialogService.DisplayAlertAsync("Message", "Distribution has been saved successfully", "OK");
                    var navigationResult = await NavigationService.GoBackToRootAsync(new NavigationParameters
                    {
                        {"RefreshMultiRequests", true }
                    });
                }
                IsBusy = false;

                //var parameters = new DialogParameters
                //{
                //    { "Title", "Message" },
                //    { "Message", jsonString }
                //};
                //await _dialogService.ShowDialogAsync("MessageDialog", parameters);
            }
            catch (Refit.ApiException apiEx)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", apiEx.Message + Environment.NewLine + (apiEx.Content ?? ""), "OK");
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public DelegateCommand GoBackCommand { get; }
        private async void OnGoBackCommand()
        {
            try
            {
                var result = await PageDialogService.DisplayAlertAsync("Warning", $"Do you really want to exit?" + Environment.NewLine
                    + "Distribution is not saved", "Yes", "No");
                if (result)
                {
                    _ = await NavigationService.GoBackAsync();
                }
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand RemoveSupport { get; }
        private async void OnRemoveSupport()
        {
            try
            {
                if (_supportList.Count == 1)
                    throw new Exception("This plate can't be removed" + Environment.NewLine + "You need at least one plate");

                var result = await PageDialogService.DisplayAlertAsync("Warning", $"Are you sure to remove this plate?", "Yes", "No");
                if (result)
                {
                    IsBusy = true;

                    //Clear selected materials
                    foreach (var cell in LocationList)
                    {
                        if (cell.IsSelected && cell.DataType == ReadingDataEntry.Entry)
                        {
                            var currentCellMaterial = MaterialList.FirstOrDefault(x => x.Item.composite_sample_id.Equals(cell.composite_sample_id));
                            if(currentCellMaterial != null)
                            {
                                currentCellMaterial.Selected = false;
                            }
                        }
                    }
                    //Select next available material
                    //var nextItem = MaterialList.FirstOrDefault(x => x.Selected == false);
                    //CurrentSampleMaterial = nextItem ?? null;

                    //re-order
                    LocationList = null;
                    _supportList.RemoveAt(SupportOrder - 1);
                    SupportCount--;

                    if (SupportOrder > _supportList.Count)
                    {
                        SupportOrder--;
                    }
                    LocationList = _supportList[SupportOrder - 1];
                    IsBusy = false;
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        public DelegateCommand PathogenChangedCommand { get; set; }
        private async void ExecutePathogenChangedCommand()
        {
            try
            {
                IsBusy = true;

                if(_isLoaded && SelectedPathogen != null)
                {
                    LocationList = _pathogenPlates[_selectedPathogen.PathogenId][_supportOrder - 1];
                    SupportCount = _pathogenPlates[_selectedPathogen.PathogenId].Count;

                    if(SelectedRequest != null && SelectedSampleType != null)
                    {
                        foreach (var selectableSample in MaterialList)
                        {
                            var sampleIsPlaced = _cachePathogenSamples.Any(x => x.agent_id == _selectedPathogen.PathogenId && x.composite_sample_id == selectableSample.Item.composite_sample_id);
                            selectableSample.Selected = sampleIsPlaced;
                        }
                        //Select next available material
                        //var nextItem = MaterialList.FirstOrDefault(x => x.Selected == false);
                        //CurrentSampleMaterial = nextItem ?? MaterialList.FirstOrDefault();
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
        public DelegateCommand PreviousSampleCommand { get; }
        private async void ExecutePreviousSampleCommand()
        {
            try
            {
                IsBusy = true;

                var iMaterial = MaterialList.IndexOf(CurrentSampleMaterial);
                if(iMaterial >= 1)
                {
                    CurrentSampleMaterial = MaterialList[--iMaterial];
                    SampleIndex = iMaterial + 1;
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
                var iMaterial = MaterialList.IndexOf(CurrentSampleMaterial);
                if (iMaterial <= MaterialList.Count - 2)
                {
                    CurrentSampleMaterial = MaterialList[++iMaterial];
                    SampleIndex = iMaterial + 1;
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
        public override async void Initialize(INavigationParameters parameters)
        {
            try
            {
                IsBusy = true;

                if (parameters.ContainsKey("ElisaEvent"))
                {
                    _elisaEvent = parameters.GetValue<ElisaEvent>("ElisaEvent");
                }
                else
                    throw new Exception($"{nameof(ElisaEvent)} is required");
                
                if (parameters.ContainsKey("SelectedRequestList"))
                {
                    var tempSelectedRequestList = parameters.GetValue<IEnumerable<RequestLookup>>("SelectedRequestList");
                    SelectedRequestList = tempSelectedRequestList;

                    Dictionary<string, string> tempColorList = new Dictionary<string, string>();
                    for (int i = 0; i < SelectedRequestList.Count(); i++)
                    {
                        //_requestColorPalette.Add(SelectedRequestList.ElementAt(i).request_id, ColorPalette.Colors[i % ColorPalette.Colors.Length]);
                        //tempColorList.Add(SelectedRequestList.ElementAt(i).request_code, ColorPalette.Colors[i % ColorPalette.Colors.Length]);

                        var request = SelectedRequestList.ElementAt(i);
                        int lastDigit = 0;
                        if (int.TryParse(request.request_code.Substring(request.request_code.Length - 1), out lastDigit))
                        {
                            _requestColorPalette.Add(SelectedRequestList.ElementAt(i).request_id, ColorPalette.Colors[lastDigit]);
                            tempColorList.Add(SelectedRequestList.ElementAt(i).formatted_request_code, ColorPalette.Colors[lastDigit]);
                        }
                        else
                            throw new Exception("Invalid request code");
                    }
                    ColorList = tempColorList;
                }
                else
                    throw new Exception($"{nameof(SelectedRequestList)} is required");
                
                if (parameters.ContainsKey("SelectedPathogenList"))
                {
                    var tempPathogenList = parameters.GetValue<IEnumerable<PathogenItem>>("SelectedPathogenList").ToList();
                    _elisaDistribution.agent_ids = tempPathogenList.Select(x => x.PathogenId).ToArray();

                    PathogenList = tempPathogenList;
                    
                    foreach (var pathogenItem in PathogenList)
                    {
                        _pathogenPlates.Add(pathogenItem.PathogenId, new List<IEnumerable<PlateLocation>>());
                    }
                }
                else
                    throw new Exception($"{nameof(PathogenList)} is required");

                if (SelectedPathogen == null)
                {
                    SelectedPathogen = PathogenList.First();
                }

                SupportOrder = 1;
                if (parameters.ContainsKey("PlatesDistributions"))
                {
                    _platesDistributions = parameters.GetValue<IEnumerable<SupportLocation>>("PlatesDistributions");
                    var maxPlateOrder = _platesDistributions.Select(x => x.support_order_id).Max();

                    foreach (var pathogenId in _pathogenPlates.Keys)
                    {
                        var pathogenPlatesDistributions = _platesDistributions.Where(x => x.agent_id == pathogenId);
                        
                        for (int plateOrder = 1; plateOrder <= maxPlateOrder; plateOrder++)
                        {
                            List<PlateLocation> tempLocationList = new List<PlateLocation>();
                            for (int iRow = 0; iRow < 8; iRow++)
                            {
                                for (int iCol = 0; iCol < 12; iCol++)
                                {
                                    var cellPosition = (iCol * 8) + iRow + 1;
                                    var cellPositionTitle = Utils.ConvertPositionToPositionTitle(iRow, iCol);
                                    var pathogenPlateCell = pathogenPlatesDistributions
                                        .FirstOrDefault(x =>
                                            x.support_order_id == plateOrder && x.support_cell_position == cellPosition);
                                    //x.reading_data_type_id == (int)ReadingDataEntry.Entry
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
                            _pathogenPlates[pathogenId].Add(tempLocationList);
                        }
                    }

                    //Check if materials are already placed
                    var placedSampleIds = _platesDistributions.Select(x => x.composite_sample_id).Distinct();
                    foreach (var placedSampleId in placedSampleIds)
                    {
                        foreach (var pathogenId in _pathogenPlates.Keys)
                        {
                            var placed = _platesDistributions.Any(x => x.agent_id == pathogenId && x.composite_sample_id == placedSampleId);
                            if (placed)
                            {
                                _cachePathogenSamples.Add(new SupportLocation
                                {
                                    agent_id = pathogenId,
                                    composite_sample_id = placedSampleId,
                                });
                            }
                        }
                    }

                    //Load distribution for first pathogen
                    if (SelectedPathogen != null)
                    {
                        LocationList = _pathogenPlates[_selectedPathogen.PathogenId][_supportOrder - 1];
                        SupportCount = _pathogenPlates[_selectedPathogen.PathogenId].Count;

                        if (SelectedRequest != null && SelectedSampleType != null)
                        {
                            foreach (var selectableSample in MaterialList)
                            {
                                var sampleIsPlaced = _cachePathogenSamples.Any(x => x.agent_id == _selectedPathogen.PathogenId && x.composite_sample_id == selectableSample.Item.composite_sample_id);
                                selectableSample.Selected = sampleIsPlaced;
                            }
                            //Select next available material
                            //var nextItem = MaterialList.FirstOrDefault(x => x.Selected == false);
                            //CurrentSampleMaterial = nextItem ?? MaterialList.FirstOrDefault();
                        }
                    }

                    //Load request process assays status
                    var requestProcessAssayIds = _platesDistributions.Select(x => x.request_process_essay_id).Distinct();
                    foreach (var reqProAssayId in requestProcessAssayIds)
                    {
                        var requestProcessAssay = await _requestService.GetRequestProcessAssay(reqProAssayId);

                        if (_cacheRequestProcessAssays.ContainsKey(requestProcessAssay.request_process_essay_id))
                            _cacheRequestProcessAssays[requestProcessAssay.request_process_essay_id] = requestProcessAssay;
                        else
                            _cacheRequestProcessAssays.Add(requestProcessAssay.request_process_essay_id, requestProcessAssay);
                    }
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
                    foreach (var pathogenId in _pathogenPlates.Keys)
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

                    LocationList = _pathogenPlates.First().Value[0];
                    SupportCount = _pathogenPlates.First().Value.Count;
                }

                _isLoaded = true;
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
    }
}
