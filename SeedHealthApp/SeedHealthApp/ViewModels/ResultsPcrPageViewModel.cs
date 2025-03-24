using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using Prism.Services.Dialogs;
using SeedHealthApp.Custom.Events;
using SeedHealthApp.Extensions;
using SeedHealthApp.Models;
using SeedHealthApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SeedHealthApp.ViewModels
{
    public class ResultsPcrPageViewModel : ViewModelBase
    {
        IRequestService _requestService;
        IToastService _toastService;
        private readonly IDialogService _dialogService;
        private IEnumerable<Result> _results;

        private List<PcrResult> _pcrResults;
        private SampleType SelectedSampleType;

        private int _selectedMaterialListIndex;
        public int SelectedMaterialListIndex
        {
            get { return _selectedMaterialListIndex; }
            set { SetProperty(ref _selectedMaterialListIndex, value); }
        }
        
        private List<SelectableModel<SampleMaterial>> _materialList;
        public List<SelectableModel<SampleMaterial>> MaterialList
        {
            get { return _materialList; }
            set { SetProperty(ref _materialList, value); }
        }
        private IEnumerable<SampleMaterial> _selectedMaterialList;
        public IEnumerable<SampleMaterial> SelectedMaterialList
        {
            get { return _selectedMaterialList; }
            set { SetProperty(ref _selectedMaterialList, value); }
        }
        private SampleMaterial _currentMaterial;
        public SampleMaterial CurrentMaterial
        {
            get { return _currentMaterial; }
            set { SetProperty(ref _currentMaterial, value); }
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

        private SampleMaterial _selectedMaterial;
        public SampleMaterial SelectedMaterial
        {
            get { return _selectedMaterial; }
            set { SetProperty(ref _selectedMaterial, value); }
        }

        private ObservableCollection<PcrAgentResult> _resultList;
        public ObservableCollection<PcrAgentResult> ResultList
        {
            get { return _resultList; }
            set { SetProperty(ref _resultList, value); }
        }
        private bool _isConfigured;
        public bool IsConfigured
        {
            get { return _isConfigured; }
            set { SetProperty(ref _isConfigured, value); }
        }

        private IEnumerable<PathogenItem> _pathogenList;
        public IEnumerable<PathogenItem> PathogenList
        {
            get { return _pathogenList; }
            set { SetProperty(ref _pathogenList, value); }
        }
        private bool _markedToReprocess;
        public bool MarkedToReprocess
        {
            get { return _markedToReprocess; }
            set { SetProperty(ref _markedToReprocess, value); }
        }

        public ResultsPcrPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService, ISettingsService settingsService,
            IRequestService requestService, IEventAggregator eventAggregator, IToastService toastService, IDialogService dialogService) 
        : base(navigationService, pageDialogService, settingsService, eventAggregator)
        {
            _toastService = toastService;
            _requestService = requestService;
            _dialogService = dialogService;

            ProcessCommand = new DelegateCommand(OnProcessCommand);
            PreviousMaterialCommand = new DelegateCommand(OnPreviousMaterialCommand);
            NextMaterialCommand = new DelegateCommand(OnNextMaterialCommand);
            SaveEvaluationCommmand = new DelegateCommand(OnSaveEvaluationCommmand);
            FinishEvaluationCommand = new DelegateCommand(OnFinishEvaluationCommand);
            CheckAllCommand = new DelegateCommand<string>(OnCheckAllCommand);

            Title = "PCR results";
            Username = SettingsService.Username;

            _pcrResults = new List<PcrResult>();
        }

        public DelegateCommand ProcessCommand { get; }
        private async void OnProcessCommand()
        {
            try
            {
                if (MaterialList.Any())
                {
                    SelectedMaterialList = MaterialList.Where(x => x.Selected).Select(x => x.Item).ToList();
                    if (!SelectedMaterialList.Any())
                    {
                        SelectedMaterialList = new List<SampleMaterial>();
                        SelectedMaterialListIndex = -1;
                        SelectedMaterial = null;
                        throw new Exception("Material list is empty");
                    }

                    SelectedMaterialListIndex = 0;
                    SelectedMaterial = SelectedMaterialList.ElementAt(SelectedMaterialListIndex);
                }
                else
                    throw new Exception("Material list is empty");

                if (!PathogenList.Any())
                    throw new Exception("Pathogen list is empty");
                var selectedPathogenList = PathogenList.Where(x => x.IsSelected == true);
                if(!selectedPathogenList.Any())
                    throw new Exception("Pathogen list is empty");

                //var results = await _requestService.GetResults(SelectedSampleType.request_process_essay_id, SelectedSampleType.IsAvailableOffline);

                _pcrResults = new List<PcrResult>();

                //Obtener resultado actual
                await LoadResults();

                /*
                foreach (var material in SelectedMaterialList)
                {
                    var tempResultList = new List<PcrAgentResult>();
                    foreach (var pathogen in selectedPathogenList)
                    {
                        tempResultList.Add(new PcrAgentResult() { AgentId = pathogen.PathogenId, AgentName = pathogen.PathogenName, IsChecked = false });
                    }

                    _pcrResults.Add(new PcrResult()
                    {
                        MaterialId = material.composite_sample_id,
                        AgentResults = tempResultList
                    });
                }
                ResultList = _pcrResults.Where(x => x.MaterialId == SelectedMaterial.composite_sample_id).FirstOrDefault().AgentResults;
                */

                IsConfigured = true;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        private async Task ProcessAsync()
        {
            if (MaterialList.Any())
            {
                SelectedMaterialList = MaterialList.Where(x => x.Selected).Select(x => x.Item).ToList();
                if (!SelectedMaterialList.Any())
                {
                    SelectedMaterialList = new List<SampleMaterial>();
                    SelectedMaterialListIndex = -1;
                    SelectedMaterial = null;
                    throw new Exception("Material list is empty");
                }

                SelectedMaterialListIndex = 0;
                SelectedMaterial = SelectedMaterialList.ElementAt(SelectedMaterialListIndex);
            }
            else
                throw new Exception("Material list is empty");

            _pcrResults = new List<PcrResult>();

            //Obtener resultado actual
            await LoadResults();
            IsConfigured = true;
        }
        private async Task LoadResults()
        {
            _results = await _requestService.GetResults(SelectedSampleType.request_process_essay_id, null, SelectedMaterial.composite_sample_id,
                SelectedSampleType.IsAvailableOffline);

            MarkedToReprocess = !_results.Any() ? true : _results.Any(x => x.reprocess);

            var selectedPathogenList = PathogenList.Where(x => x.IsSelected);
            var tempResultList = new List<PcrAgentResult>();
            foreach (var pathogen in selectedPathogenList)
            {
                var found = _results.FirstOrDefault(x => x.composite_sample_id == SelectedMaterial.composite_sample_id && x.agent_id == pathogen.PathogenId);
                if (found == null)
                    tempResultList.Add(new PcrAgentResult()
                    {
                        AgentId = pathogen.PathogenId,
                        AgentName = pathogen.PathogenName,
                        IsChecked = false });
                else
                    tempResultList.Add(new PcrAgentResult()
                    {
                        AgentId = pathogen.PathogenId,
                        AgentName = pathogen.PathogenName,
                        IsChecked = found.text_result.Equals("positive")
                    });
            }
            ResultList = new ObservableCollection<PcrAgentResult>(tempResultList);
        }
        public DelegateCommand<string> CheckAllCommand { get; }
        private async void OnCheckAllCommand(string param)
        {
            try
            {
                if (param.Equals("materials"))
                {
                    foreach (var item in MaterialList)
                    {
                        item.Selected = true;
                    }                    
                }
                else if(param.Equals("pathogens"))
                {
                    foreach (var item in PathogenList)
                    {
                        item.IsSelected = true;
                    }
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await PageDialogService.DisplayAlertAsync("Error", ex.Message + Environment.NewLine + ex.InnerException?.Message, "OK");
            }
        }
        public DelegateCommand PreviousMaterialCommand { get; }
        private async void OnPreviousMaterialCommand()
        {
            try
            {
                IsBusy = true;

                if (SelectedMaterialListIndex <= 0)
                {
                    return;
                }
                
                SelectedMaterialListIndex--;
                SelectedMaterial = SelectedMaterialList.ElementAt(SelectedMaterialListIndex);

                await LoadResults();
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
        private async void OnNextMaterialCommand()
        {
            try
            {
                IsBusy = true;

                if (SelectedMaterialListIndex >= SelectedMaterialList.Count() - 1)
                {
                    return;
                }

                SelectedMaterialListIndex++;
                SelectedMaterial = SelectedMaterialList.ElementAt(SelectedMaterialListIndex);

                await LoadResults();
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
                /*
                if (SelectedSampleType.IsAvailableOffline) //Save all at once
                {
                    var modifiedResults = ResultList.Select(result => new Result()
                    {
                        request_process_assay_id = SelectedSampleType.request_process_essay_id,
                        activity_id = 18,
                        composite_sample_id = SelectedMaterial.composite_sample_id,
                        agent_id = result.AgentId,
                        auxiliary_result = result.IsChecked ? "positive" : "negative",
                        number_result = null,
                        text_result = result.IsChecked ? "positive" : "negative",
                    });

                    await PageDialogService.DisplayAlertAsync("",
                        Newtonsoft.Json.JsonConvert.SerializeObject(modifiedResults, Newtonsoft.Json.Formatting.Indented),
                        "OK");
                }
                else //Save by batch
                {*/
                    //var watch = System.Diagnostics.Stopwatch.StartNew();
                    float ProgressIndicator = 0.1f;

                    float delta = (float)(0.9 / ResultList.Count());
                    int batchSize = 5;
                    int numberOfBatches = (int)Math.Ceiling((double)ResultList.Count() / batchSize);
                    try
                    {
                        var tasks = new List<Task>();
                        for (int i = 0; i < numberOfBatches; i++)
                        {
                            var pcrAgentResults = ResultList.Skip(i * batchSize).Take(batchSize);
                            foreach (var result in pcrAgentResults)
                            {
                                async Task func()
                                {
                                    await _requestService.CreateResult(SelectedSampleType.request_process_essay_id, new Result()
                                    {
                                        request_process_assay_id = SelectedSampleType.request_process_essay_id,
                                        activity_id = 18,
                                        composite_sample_id = SelectedMaterial.composite_sample_id,
                                        agent_id = result.AgentId,
                                        auxiliary_result = result.IsChecked ? "positive" : "negative",
                                        number_result = null,
                                        text_result = result.IsChecked ? "positive" : "negative",
                                        reprocess = MarkedToReprocess,
                                    }, SelectedSampleType.IsAvailableOffline);
                                }
                                tasks.Add(func());
                            }
                            await Task.WhenAll(tasks);
                            ProgressIndicator += delta * batchSize;
                        }
                        EventAggregator.GetEvent<ResultUpdatedEvent>().Publish();
                    }
                    catch (Refit.ApiException apiEx)
                    {
                        throw new Exception(apiEx.Message + Environment.NewLine + apiEx.Content);
                    }
                    catch (Exception ex1)
                    {
                        throw new Exception("An error occurred while saving" + Environment.NewLine +
                            ex1.Message + Environment.NewLine + ex1.InnerException?.Message);
                    }
                    finally
                    {
                        //watch.Stop();
                    }
                //}

                //Refresh results
                _results = await _requestService.GetResults(SelectedSampleType.request_process_essay_id, null, SelectedMaterial.composite_sample_id,
                SelectedSampleType.IsAvailableOffline);

                IsBusy = false;
                _toastService.ShowToast("Saved succesfully");
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
                if(apiResult.Status == 200)
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
        private DelegateCommand _choosePathogensCommand;

        public DelegateCommand ChoosePathogensCommand =>
            _choosePathogensCommand ?? (_choosePathogensCommand = new DelegateCommand(ExecuteChoosePathogensCommand));

        private async void ExecuteChoosePathogensCommand()
        {
            try
            {
                IsBusy = true;

                DialogParameters dialogParameters = new DialogParameters
                {
                    { "PathogenItemList", new ObservableCollection<PathogenItem>(PathogenList)},
                    { "IsSearchVisible", false},
                    { "ShowCheckAll", true}
                };
                var result = await _dialogService.ShowDialogAsync("PathogenPickerDialog", dialogParameters);
                if (result.Parameters.ContainsKey("SelectedPathogenItemList"))
                {
                    var selectedPathogenItems = (IEnumerable<PathogenItem>)result.Parameters["SelectedPathogenItemList"];

                    foreach (var item in PathogenList)
                    {
                        item.IsSelected = selectedPathogenItems.Any(x => x.PathogenId == item.PathogenId);
                    }

                    //Remove unselected pathogens
                    for (int i = ResultList.Count() - 1; i >= 0; i--)
                    {
                        var isFound = selectedPathogenItems.Any(x => x.PathogenId == ResultList[i].AgentId);
                        if (!isFound)
                        {
                            ResultList.RemoveAt(i);
                        }
                    }

                    //add
                    foreach(var pathogen in selectedPathogenItems)
                    {
                        var found = ResultList.Any(x => x.AgentId == pathogen.PathogenId);
                        if (!found)
                        {
                            var pathogenResult = _results.FirstOrDefault(x => x.agent_id == pathogen.PathogenId);
                            if(pathogenResult == null)
                            {
                                ResultList.Add(new PcrAgentResult()
                                {
                                    AgentId = pathogen.PathogenId,
                                    AgentName = pathogen.PathogenName,
                                    IsChecked = false
                                });
                            }
                            else
                            {
                                ResultList.Add(new PcrAgentResult()
                                {
                                    AgentId = pathogen.PathogenId,
                                    AgentName = pathogen.PathogenName,
                                    IsChecked = pathogenResult.text_result.Equals("positive")
                                });
                            }
                        }
                    }

                    //ResultList = new ObservableCollection<PcrAgentResult>(ResultList.OrderBy(x => x.AgentName));
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

                if (parameters.ContainsKey("SelectedSampleType"))
                {
                    SelectedSampleType = (SampleType)parameters["SelectedSampleType"];
                }
                if (parameters.ContainsKey("SelectedActiveAssaySampleType"))
                {
                    SelectedSampleType = (SampleType)parameters["SelectedActiveAssaySampleType"];
                }

                if (parameters.ContainsKey("SelectedActivity"))
                {
                    SelectedActivity = (Activity)parameters["SelectedActivity"];
                }

                var materials = await _requestService.GetRequestProcessAssaySamples(SelectedSampleType.request_process_essay_id);
                
                var tempMaterialList = new List<SelectableModel<SampleMaterial>>();
                foreach (var material in materials)
                {
                    if (material.num_order.HasValue)
                        tempMaterialList.Add(new SelectableModel<SampleMaterial>() { Item = material, Selected = true});
                }
                MaterialList = tempMaterialList;
                

                /************************/
                var agents = await _requestService.GetAgents(Request.request_id, SelectedSampleType.process_id, Essay.assay_id, SelectedSampleType.sample_type_id);

                var tempPathogenItemList = new List<PathogenItem>();
                foreach (var agent in agents)
                {
                    tempPathogenItemList.Add(new PathogenItem()
                    {
                        PathogenId = agent.agent_id,
                        PathogenName = agent.agent_name,
                        IsSelected = false,
                    });
                }
                PathogenList = new List<PathogenItem>(tempPathogenItemList);

                IsConfigured = true;
                await ProcessAsync();
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
    }
}
