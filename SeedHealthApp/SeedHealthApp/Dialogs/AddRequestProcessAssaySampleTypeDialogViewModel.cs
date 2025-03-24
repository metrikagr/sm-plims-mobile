using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using SeedHealthApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Dialogs
{
    public class AddRequestProcessAssaySampleTypeDialogViewModel : BindableBase, IDialogAware
    {
        public event Action<IDialogParameters> RequestClose;
        #region
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }
        private IEnumerable<SampleType> _inactiveSampleList;
        public IEnumerable<SampleType> InactiveSampleList
        {
            get { return _inactiveSampleList; }
            set { SetProperty(ref _inactiveSampleList, value); }
        }
        private IEnumerable<int> _repetitionList;
        public IEnumerable<int> RepetitionList
        {
            get { return _repetitionList; }
            set { SetProperty(ref _repetitionList, value); }
        }
        private IEnumerable<ContainerType> _containerTypeList;
        public IEnumerable<ContainerType> ContainerTypeList
        {
            get { return _containerTypeList; }
            set { SetProperty(ref _containerTypeList, value); }
        }
        private bool _isRepetitionCountVisible;
        public bool IsRepetitionCountVisible
        {
            get { return _isRepetitionCountVisible; }
            set { SetProperty(ref _isRepetitionCountVisible, value); }
        }
        private bool _isContainerTypeVisible;
        public bool IsContainerTypeVisible
        {
            get { return _isContainerTypeVisible; }
            set { SetProperty(ref _isContainerTypeVisible, value); }
        }
        private SampleType _SelectedSampleType;
        public SampleType SelectedSampleType
        {
            get { return _SelectedSampleType; }
            set { SetProperty(ref _SelectedSampleType, value); }
        }
        private int? _selectedActivityCount;
        public int? SelectedActivityCount
        {
            get { return _selectedActivityCount; }
            set { SetProperty(ref _selectedActivityCount, value); }
        }
        private ContainerType _selectedContainerType;
        public ContainerType SelectedContainerType
        {
            get { return _selectedContainerType; }
            set { SetProperty(ref _selectedContainerType, value); }
        }

        #endregion

        public AddRequestProcessAssaySampleTypeDialogViewModel()
        {
            CloseCommand = new DelegateCommand(() => RequestClose(null));
            AcceptCommand = new DelegateCommand(OnAcceptCommand);
        }

        public DelegateCommand CloseCommand { get; }
        public DelegateCommand AcceptCommand { get; }
        private void OnAcceptCommand()
        {
            try
            {
                Message = string.Empty;

                if (SelectedSampleType == null)
                    throw new Exception("Sample type is empty");
                if (IsRepetitionCountVisible && SelectedActivityCount == null)
                    throw new Exception("Repetition count is empty");
                if (IsContainerTypeVisible && SelectedContainerType == null)
                    throw new Exception("Container type is empty");

                RequestClose(new DialogParameters()
                {
                    { "SelectedSampleType", SelectedSampleType },
                    { "SelectedActivityCount", SelectedActivityCount },
                    { "SelectedContainerType", SelectedContainerType },
                });
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                Console.WriteLine("Error : " + ex.Message);
            }
        }
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            try
            {
                IsBusy = true;

                if (parameters.ContainsKey("InactiveSampleTypeList"))
                {
                    InactiveSampleList = parameters.GetValue<IEnumerable<SampleType>>("InactiveSampleTypeList");
                    RepetitionList = parameters.GetValue<IEnumerable<int>>("RepetitionList");
                    ContainerTypeList = parameters.GetValue<IEnumerable<ContainerType>>("ContainerTypeList");
                    IsRepetitionCountVisible = parameters.GetValue<bool>("IsRepetitionCountVisible");
                    IsContainerTypeVisible = parameters.GetValue<bool>("IsContainerTypeVisible");
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                Message = ex.Message;
                Console.WriteLine(ex.Message);
            }
        }
    }
}
