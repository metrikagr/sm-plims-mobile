using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using SeedHealthApp.Custom.Events;
using SeedHealthApp.Extensions;
using SeedHealthApp.Models;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SeedHealthApp.ViewModels
{
    public class MultiRequestPageViewModel : ViewModelBase
    {
        private readonly IRequestService _requestService;
        private ElisaEvent _elisaEvent;
        private List<SupportLocation> _platesDistributions = new List<SupportLocation>();
        private Dictionary<int, string> _requestColorPalette = new Dictionary<int, string>();

        private IEnumerable<PathogenItem> _pathogenList;
        public IEnumerable<PathogenItem> PathogenList
        {
            get { return _pathogenList; }
            set { SetProperty(ref _pathogenList, value); }
        }
        private IEnumerable<SelectableModel<SampleType>> _requestProcessAssayList;
        public IEnumerable<SelectableModel<SampleType>> RequestProcessAssayList
        {
            get { return _requestProcessAssayList; }
            set { SetProperty(ref _requestProcessAssayList, value); }
        }
        private List<ElisaPathogen> _elisaPathogenList;
        public List<ElisaPathogen> ElisaPathogenList
        {
            get { return _elisaPathogenList; }
            set { SetProperty(ref _elisaPathogenList, value); }
        }
        private Xamarin.Forms.SelectionMode selectionMode;
        public Xamarin.Forms.SelectionMode SelectionMode
        {
            get { return selectionMode; }
            set { SetProperty(ref selectionMode, value); }
        }
        private ObservableCollection<object> _selectedElisaPathogenList;
        public ObservableCollection<object> SelectedElisaPathogenList
        {
            get { return _selectedElisaPathogenList; }
            set { SetProperty(ref _selectedElisaPathogenList, value); }
        }
        private ElisaPathogen _selectedElisaPathogen;
        public ElisaPathogen SelectedElisaPathogen
        {
            get { return _selectedElisaPathogen; }
            set { SetProperty(ref _selectedElisaPathogen, value); }
        }
        private bool _isEditable;
        public bool IsEditable
        {
            get { return _isEditable; }
            set { SetProperty(ref _isEditable, value); }
        }
        public Dictionary<int, int> RequestProcessAssayLastSamples { get; set; } = new Dictionary<int, int>();
        public MultiRequestPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService,
            ISettingsService settingsService, IEventAggregator eventAggregator,
            IRequestService requestService) :
            base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _requestService = requestService;
            Username = SettingsService.Username;

            ElisaPathogenList = new List<ElisaPathogen>();//Enumerable.Empty<ElisaPathogen>();
            SelectedElisaPathogenList = new ObservableCollection<object>();
            SelectionMode = Xamarin.Forms.SelectionMode.Single;

            RefreshProcessAssayListCommand = new DelegateCommand(ExecuteRefreshProcessAssayListCommand).ObservesCanExecute(() => IsIdle);
            SetDistributionCommand = new DelegateCommand(ExecuteSetDistributionCommand).ObservesCanExecute(() => IsIdle);
            PathogenItemTappedCommand = new DelegateCommand<object>(ExecutePathogenItemTappedCommand).ObservesCanExecute(() => IsIdle);

            ChangeSelectionModeCommand = new DelegateCommand(ExecuteChangeSelectionModeCommand);
            EditPlateCommand = new DelegateCommand(ExecuteEditPlateCommand).ObservesCanExecute(() => IsIdle);
            ElisaPlateSelectionChangedCommand = new DelegateCommand<object>(ExecuteElisaPlateSelectionChangedCommand).ObservesCanExecute(() => IsIdle);
            ShowDebugInfoCommand = new DelegateCommand(ExecuteShowDebugInfoCommand).ObservesCanExecute(() => IsIdle);
            AddPlateCommand = new DelegateCommand(ExecuteAddPlateCommand).ObservesCanExecute(() => IsIdle);

            _ = EventAggregator.GetEvent<ElisaPlateDistributionSavedEvent>().Subscribe(ElisaPlateDistributionSaved);
            //_ = EventAggregator.GetEvent<MultirequestCurrentSampleChangedEvent>().Subscribe(MultirequestCurrentSampleChanged);
        }
        public DelegateCommand ShowDebugInfoCommand { get; }
        private async void ExecuteShowDebugInfoCommand()
        {
            try
            {
                IsBusy = true;
                await PageDialogService.DisplayAlertAsync("", JsonConvert.SerializeObject(_elisaEvent, Formatting.Indented), "OK");
                await PageDialogService.DisplayAlertAsync("", JsonConvert.SerializeObject(_elisaPathogenList, Formatting.Indented), "OK");
                await PageDialogService.DisplayAlertAsync("", JsonConvert.SerializeObject(_platesDistributions, Formatting.Indented), "OK");
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
        
        private async void ElisaPlateDistributionSaved(string message)
        {
            try
            {
                IsBusy = true;
                var parameters = message.Split('|');
                var strPathogenIds = parameters[0];
                var strPlateOrder = parameters[1];

                var modifiedPlateCells = await _requestService.GetSupportLocationsAsync(_elisaEvent.event_id, strPathogenIds, strPlateOrder);
                var pathogenIds = strPathogenIds.Split(',').Select(x => int.Parse(x)).ToArray();
                var toRefresh = _platesDistributions.Where(x => x.support_order_id == int.Parse(strPlateOrder) && pathogenIds.Contains(x.agent_id)).ToList();
                foreach (var item in toRefresh)
                {
                    _platesDistributions.Remove(item);
                }
                _platesDistributions.AddRange(modifiedPlateCells);

                foreach (var pathogenId in pathogenIds)
                {
                    var plateCells = new List<PlateCell>();
                    foreach (var foundCell in _platesDistributions.Where(x => x.agent_id == pathogenId && x.support_order_id == int.Parse(strPlateOrder)))
                    {
                        if (!plateCells.Any(x => x.Position == foundCell.support_cell_position))
                        {
                            string color;
                            switch (foundCell.reading_data_type_id)
                            {
                                case (int)ReadingDataEntry.Entry: color = _requestColorPalette[foundCell.request_id]; break;
                                case (int)ReadingDataEntry.Positive: color = "positive"; break;
                                case (int)ReadingDataEntry.Negative: color = "negative"; break;
                                case (int)ReadingDataEntry.Buffer: color = "buffer"; break;
                                default:
                                    throw new Exception("Reading data type not supported");
                            }
                            plateCells.Add(new PlateCell { Position = foundCell.support_cell_position, Color = color });
                        }
                    }
                    ElisaPathogenList.First(x => x.PathogenId == pathogenId)
                        .PlateList.First(y => y.PlateOrder == int.Parse(strPlateOrder))
                        .PlateCells = plateCells;
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
        public DelegateCommand ChangeSelectionModeCommand { get; }
        private void ExecuteChangeSelectionModeCommand()
        {
            if(SelectionMode == Xamarin.Forms.SelectionMode.Single)
            {
                SelectedElisaPathogen = null;
                SelectionMode = Xamarin.Forms.SelectionMode.Multiple;
            }
            else
            {
                SelectedElisaPathogenList.Clear();
                SelectionMode = Xamarin.Forms.SelectionMode.Single;
            }
            foreach (var elisaPathogen in ElisaPathogenList)
            {
                elisaPathogen.SelectedElisaPlate = null;
            }
        }
        public DelegateCommand EditPlateCommand { get; }
        private async void ExecuteEditPlateCommand()
        {
            try
            {
                IsBusy = true;

                if (!IsEditable) return;

                if (SelectionMode == Xamarin.Forms.SelectionMode.Single)
                {
                    if (SelectedElisaPathogen == null)
                        throw new Exception("Selected pathogen is empty");

                    if (SelectedElisaPathogen.SelectedElisaPlate == null)
                        SelectedElisaPathogen.SelectedElisaPlate = SelectedElisaPathogen.PlateList.FirstOrDefault();

                    var requestProcessAssayList = await _requestService.GetElisaRequestProcessAssaysByAgents(_elisaEvent.event_id, new int[] { SelectedElisaPathogen.PathogenId});
                    requestProcessAssayList = requestProcessAssayList.OrderBy(x => x.RequestCodeSampleType).ToList();

                    //await PageDialogService.DisplayAlertAsync("Selected pathogen", Newtonsoft.Json.JsonConvert.SerializeObject(SelectedElisaPathogen, Newtonsoft.Json.Formatting.Indented),"OK");
                    //await PageDialogService.DisplayAlertAsync("Selected pathogen", Newtonsoft.Json.JsonConvert.SerializeObject(requestProcessAssayList, Newtonsoft.Json.Formatting.Indented),"OK");

                    var pathogenElisaDistribution = _platesDistributions.Where(x => x.agent_id == SelectedElisaPathogen.PathogenId).ToList();
                    var navResult = await NavigationService.NavigateAsync("ElisaPlateDistributionPage", new NavigationParameters()
                    {
                        { "ElisaEvent", _elisaEvent },
                        { "RequestProcessAssayList", requestProcessAssayList},
                        { "PlatesDistributions", pathogenElisaDistribution},
                        { "PathogenIds", new int[]{ SelectedElisaPathogen.PathogenId } },
                        { "PathogenNames", new string[]{ SelectedElisaPathogen.PathogenName } },
                        { "PlateOrder", SelectedElisaPathogen.SelectedElisaPlate.PlateOrder },
                        { "RequestProcessAssayLastSamples", RequestProcessAssayLastSamples},
                    });
                    if (!navResult.Success)
                        throw navResult.Exception;
                }
                else
                {
                    if(!SelectedElisaPathogenList.Any())
                        throw new Exception("Selected pathogen list is empty");

                    var selectedPathogenIds = SelectedElisaPathogenList.Cast<ElisaPathogen>().Select(x => x.PathogenId).ToArray();
                    var selectedPathogenNames = SelectedElisaPathogenList.Cast<ElisaPathogen>().Select(x => x.PathogenName).ToArray();
                    var requestProcessAssayList = await _requestService.GetElisaRequestProcessAssaysByAgents(_elisaEvent.event_id, selectedPathogenIds);
                    //await PageDialogService.DisplayAlertAsync("Selected pathogen", Newtonsoft.Json.JsonConvert.SerializeObject(SelectedElisaPathogenList, Newtonsoft.Json.Formatting.Indented), "OK");

                    var plateOrder = (SelectedElisaPathogenList.First() as ElisaPathogen).SelectedElisaPlate.PlateOrder;
                    var pathogenElisaDistribution = _platesDistributions.Where(x => selectedPathogenIds.Contains(x.agent_id)).ToList();
                    var navResult = await NavigationService.NavigateAsync("ElisaPlateDistributionPage", new NavigationParameters()
                    {
                        { "ElisaEvent", _elisaEvent },
                        { "RequestProcessAssayList", requestProcessAssayList},
                        { "PlatesDistributions", pathogenElisaDistribution},
                        { "PathogenIds", selectedPathogenIds },
                        { "PathogenNames", selectedPathogenNames },
                        { "PlateOrder", plateOrder },
                        { "RequestProcessAssayLastSamples", RequestProcessAssayLastSamples},
                    });
                    if (!navResult.Success)
                        throw navResult.Exception;
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
        public DelegateCommand<object> ElisaPlateSelectionChangedCommand { get; }
        private async void ExecuteElisaPlateSelectionChangedCommand(object selectedElisaPlate)
        {
            try
            {
                IsBusy = true;

                if (selectedElisaPlate == null)
                    return;

                if (SelectionMode == Xamarin.Forms.SelectionMode.Single)
                {
                    var selectedPlate = (ElisaPlate)selectedElisaPlate;
                    var found = false;
                    foreach (var elisaPathogen in ElisaPathogenList)
                    {
                        if (!found)
                        {
                            var selected = elisaPathogen.PlateList.FirstOrDefault(x => x.ElisaPlateCode.Equals(selectedPlate.ElisaPlateCode));
                            if (selected != null)
                            {
                                if (SelectedElisaPathogen != elisaPathogen)
                                    SelectedElisaPathogen = elisaPathogen;
                                found = true;
                            }
                            else
                            {
                                elisaPathogen.SelectedElisaPlate = null;
                            }
                        }
                        else //Clear other selected plates
                        {
                            elisaPathogen.SelectedElisaPlate = null;
                        }
                    }
                }
                else
                {
                    var selectedPlate = (ElisaPlate)selectedElisaPlate;
                    foreach (var elisaPathogen in ElisaPathogenList)
                    {
                        var selected = elisaPathogen.PlateList.FirstOrDefault(x => x.ElisaPlateCode.Equals(selectedPlate.ElisaPlateCode));
                        if (selected != null)
                        {
                            if (!SelectedElisaPathogenList.Contains(elisaPathogen))
                                SelectedElisaPathogenList.Add(elisaPathogen);
                            break;
                        }
                    }
                    foreach (var selectedElisaPathogen in SelectedElisaPathogenList.Cast<ElisaPathogen>())
                    {
                        selectedElisaPathogen.SelectedElisaPlate = selectedElisaPathogen.PlateList.FirstOrDefault(x => x.PlateOrder == selectedPlate.PlateOrder);
                    }
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
        public DelegateCommand SetDistributionCommand { get; }
        private async void ExecuteSetDistributionCommand()
        {
            try
            {
                var navResult = await NavigationService.NavigateAsync("ElisaDistributionPage", new NavigationParameters
                {
                    {"ElisaEvent", _elisaEvent },
                    {"SelectedPathogenList", PathogenList },
                    {"SelectedRequestList", new RequestLookup[]{ } },
                });
                if (!navResult.Success)
                    throw navResult.Exception;
            }
            catch (Exception ex)
            {
                await PageDialogService.DisplayErrorAlertAsync(ex);
            }
        }
        public DelegateCommand RefreshProcessAssayListCommand { get; }
        private async void ExecuteRefreshProcessAssayListCommand()
        {
            try
            {
                IsBusy = true;

                var selectedPathogenIds = PathogenList.Where(x => x.IsSelected).Select(x=>x.PathogenId).ToArray();
                var requestProcessAssayList = await _requestService.GetElisaRequestProcessAssaysByAgents(_elisaEvent.event_id, selectedPathogenIds);
                RequestProcessAssayList = requestProcessAssayList.Select(x => new SelectableModel<SampleType> { Item = x, Selected = false }).ToArray();
                
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
        public DelegateCommand<object> PathogenItemTappedCommand { get; }
        private async void ExecutePathogenItemTappedCommand(object pathogenItem)
        {
            try
            {
                IsBusy = true;

                var pathogenItemTapped = (PathogenItem)pathogenItem;
                pathogenItemTapped.IsSelected = !pathogenItemTapped.IsSelected;

                var selectedPathogenIds = PathogenList.Where(x => x.IsSelected).Select(x => x.PathogenId).ToArray();
                
                if (selectedPathogenIds.Any())
                {
                    var requestProcessAssayList = await _requestService.GetElisaRequestProcessAssaysByAgents(_elisaEvent.event_id, selectedPathogenIds);
                    RequestProcessAssayList = requestProcessAssayList.Select(x => new SelectableModel<SampleType> { Item = x, Selected = false }).ToArray();
                }
                else
                    RequestProcessAssayList = new List<SelectableModel<SampleType>>();
            }
            catch (Exception ex)
            {
                RequestProcessAssayList = new List<SelectableModel<SampleType>>();
                await PageDialogService.DisplayErrorAlertAsync(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
        public DelegateCommand AddPlateCommand { get; }
        private async void ExecuteAddPlateCommand()
        {
            try
            {
                IsBusy = true;

                if (!IsEditable) return;

                if (SelectionMode == Xamarin.Forms.SelectionMode.Single)
                {
                    if (SelectedElisaPathogen == null)
                        return;
                    var maxPlate = SelectedElisaPathogen.PlateList.Max(x => x.PlateOrder);
                    SelectedElisaPathogen.PlateList.Add(new ElisaPlate
                    {
                        PlateOrder = maxPlate + 1,
                        Thumbnail = "empty_plate_thumbnail.png",
                        ElisaPlateCode = $"{SelectedElisaPathogen.PathogenId}-{maxPlate + 1}"
                    });
                }
                else
                {
                    if (!SelectedElisaPathogenList.Any())
                        return;
                    foreach (var selectedElisaPathogen in SelectedElisaPathogenList.Cast<ElisaPathogen>())
                    {
                        var maxPlate = selectedElisaPathogen.PlateList.Max(x => x.PlateOrder);
                        selectedElisaPathogen.PlateList.Add(new ElisaPlate
                        {
                            PlateOrder = maxPlate + 1,
                            Thumbnail = "empty_plate_thumbnail.png",
                            ElisaPlateCode = $"{selectedElisaPathogen.PathogenId}-{maxPlate + 1}"
                        });
                    }
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
            if (parameters.ContainsKey("ElisaEvent"))
            {
                _elisaEvent = parameters.GetValue<ElisaEvent>("ElisaEvent");
                Title = $"Multirequest {_elisaEvent.event_code}";
                IsEditable = _elisaEvent.status_id == 41;
                foreach (var requestProcess in _elisaEvent.request_process)
                {
                    if (int.TryParse(requestProcess.request_code.Substring(requestProcess.request_code.Length - 1), out int lastDigit))
                    {
                        if (!_requestColorPalette.ContainsKey(requestProcess.request_id))
                            _requestColorPalette.Add(requestProcess.request_id, ColorPalette.Colors[lastDigit]);
                    }
                    else
                        throw new Exception("Invalid request code");
                }
            }
            else
                throw new Exception($"{nameof(ElisaEvent)} is required");

            if (parameters.ContainsKey("SelectedPathogenList"))
            {
                PathogenList = parameters.GetValue<IEnumerable<PathogenItem>>("SelectedPathogenList");
            }
            else
                throw new Exception($"{nameof(PathogenList)} is required");
            if (parameters.ContainsKey("PlatesDistributions"))
            {
                _platesDistributions = parameters.GetValue<List<SupportLocation>>("PlatesDistributions");
                var maxPlateOrder = _platesDistributions.Max(x => x.support_order_id);

                var tempElisaPathogenList = PathogenList.Select(x => new ElisaPathogen()
                {
                    PathogenId = x.PathogenId,
                    PathogenName = x.PathogenName,
                }).ToList();
                foreach (var tempElisaPathogen in tempElisaPathogenList)
                {
                    var pathogenElisaPlates = new List<ElisaPlate>();
                    for (int plateOrder = 1; plateOrder <= maxPlateOrder; plateOrder++)
                    {
                        var plateCells = new List<PlateCell>();
                        foreach (var foundCell in _platesDistributions.Where(x => x.agent_id == tempElisaPathogen.PathogenId && x.support_order_id == plateOrder))
                        {
                            if (!plateCells.Any(x => x.Position == foundCell.support_cell_position))
                            {
                                string color;
                                switch (foundCell.reading_data_type_id)
                                {
                                    case (int)ReadingDataEntry.Entry: color = _requestColorPalette[foundCell.request_id]; break;
                                    case (int)ReadingDataEntry.Positive: color = "positive"; break;
                                    case (int)ReadingDataEntry.Negative: color = "negative"; break;
                                    case (int)ReadingDataEntry.Buffer: color = "buffer"; break;
                                    default:
                                        throw new Exception("Reading data type not supported");
                                }
                                plateCells.Add(new PlateCell { Position = foundCell.support_cell_position, Color = color });
                            }
                        }
                        //var hasAnyCells = _platesDistributions.Any(x => x.agent_id == tempElisaPathogen.PathogenId && x.support_order_id == plateOrder);
                        pathogenElisaPlates.Add(new ElisaPlate
                        {
                            PlateOrder = plateOrder,
                            //Thumbnail = hasAnyCells ? "non_empty_plate_thumbnail.png" : "empty_plate_thumbnail.png",
                            ElisaPlateCode = $"{tempElisaPathogen.PathogenId}-{plateOrder}",
                            PlateCells = plateCells
                        });
                    }
                    tempElisaPathogen.PlateList = new ObservableCollection<ElisaPlate>(pathogenElisaPlates);
                }
                ElisaPathogenList = tempElisaPathogenList;
            }
            else //Add first plate if empty
            {
                ElisaPathogenList = PathogenList.Select(x => new ElisaPathogen()
                {
                    PathogenId = x.PathogenId,
                    PathogenName = x.PathogenName,
                    PlateList = new ObservableCollection<ElisaPlate>(new List<ElisaPlate>
                        {
                            new ElisaPlate()
                            {
                                PlateOrder = 1,
                                ElisaPlateCode = $"{x.PathogenId}-1"
                            }
                        })
                }).ToList();
            }
        }
        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                IsBusy = true;
                if (!IsLoaded)
                {

                    IsLoaded = true;
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
        
    }
}
