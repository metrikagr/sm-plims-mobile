using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using SeedHealthApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeedHealthApp.Dialogs
{
    public class RequestPickerDialogViewModel : BindableBase, IDialogAware
    {
        public event Action<IDialogParameters> RequestClose;
        private string title = "Alert";
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private string message;
        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        private IEnumerable<SelectableModel<RequestLookup>> _requestList;
        public IEnumerable<SelectableModel<RequestLookup>> RequestList
        {
            get { return _requestList; }
            set { SetProperty(ref _requestList, value); }
        }
        public RequestPickerDialogViewModel()
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
                RequestClose(new DialogParameters() {
                { 
                    "SelectedRequestList", RequestList.Where(x=> x.Selected)}
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            Console.WriteLine("RequestPickerDialog has been closed");
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            try
            {
                //Title = parameters.GetValue<string>("Title");
                //Message = parameters.GetValue<string>("Message");
                RequestList = parameters.GetValue<IEnumerable<SelectableModel<RequestLookup>>>("RequestList");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }
    }
}
