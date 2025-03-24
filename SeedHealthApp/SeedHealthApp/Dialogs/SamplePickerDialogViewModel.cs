using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using SeedHealthApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SeedHealthApp.Dialogs
{
    public class SamplePickerDialogViewModel : BindableBase, IDialogAware
    {
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set { SetProperty(ref _searchText, value); }
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
        private bool _isSearchVisible;
        public bool IsSearchVisible
        {
            get { return _isSearchVisible; }
            set { SetProperty(ref _isSearchVisible, value); }
        }
        private bool _showCheckAll;
        public bool ShowCheckAll
        {
            get { return _showCheckAll; }
            set { SetProperty(ref _showCheckAll, value); }
        }
        public SamplePickerDialogViewModel()
        {
            CloseCommand = new DelegateCommand(OnCloseCommand);
            AcceptCommand = new DelegateCommand(OnAcceptCommand);
            //FilterPathogenCommand = new DelegateCommand(OnFilterPathogenCommand);
            //TextChangedCommand = new DelegateCommand(OnTextChangedCommand);

            Title = "";

            IsSearchVisible = true;
            ShowCheckAll = false;
        }
        public DelegateCommand CloseCommand { get; }
        private void OnCloseCommand()
        {
            RequestClose(null);
        }
        public DelegateCommand AcceptCommand { get; }
        private void OnAcceptCommand()
        {
            try
            {
                if (IsBusy)
                    return;

                if (SelectedSelectableSample == null)
                    return;

                IsBusy = true;

                RequestClose(new DialogParameters() {
                    { "SelectedSelectableSample", SelectedSelectableSample}
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
        public DelegateCommand FilterPathogenCommand { get; }
        private void OnFilterPathogenCommand()
        {
            try
            {
                //if (string.IsNullOrEmpty(SearchText))
                //{
                //    foreach (var item in PathogenItemList)
                //    {
                //        item.IsVisible = true;
                //    }
                //}
                //else
                //{
                //    foreach (var item in PathogenItemList)
                //    {
                //        item.IsVisible = false;
                //    }
                //    var pathogenList = PathogenItemList.Where(x => x.PathogenName.ToLower().Contains(SearchText.Trim().ToLower()));

                //    foreach (var item in pathogenList)
                //    {
                //        item.IsVisible = true;
                //    }
                //}
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
        public DelegateCommand TextChangedCommand { get; }
        private void OnTextChangedCommand()
        {
            try
            {
                IsBusy = true;
                if (string.IsNullOrEmpty(SearchText))
                {
                    //foreach (var item in PathogenItemList)
                    //{
                    //    item.IsVisible = true;
                    //}
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
        public event Action<IDialogParameters> RequestClose;

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
                if (parameters.ContainsKey("SelectableSampleList"))
                {
                    SelectableSampleList = parameters.GetValue<List<SelectableModel<SampleMaterial>>>("SelectableSampleList");
                }

                if (parameters.ContainsKey("IsSearchVisible"))
                {
                    IsSearchVisible = parameters.GetValue<bool>("IsSearchVisible");
                }
                if (parameters.ContainsKey("ShowCheckAll"))
                {
                    ShowCheckAll = parameters.GetValue<bool>("ShowCheckAll");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
