using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedHealthApp.Dialogs
{
    public class MessageDialogViewModel : BindableBase, IDialogAware
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

        public DelegateCommand CloseCommand { get; }

        public MessageDialogViewModel()
        {
            CloseCommand = new DelegateCommand(() => RequestClose(null));
        }
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            Console.WriteLine("Message Dialog has been closed");
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            try
            {
                Message = parameters.GetValue<string>("Message");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error : " + ex.Message);
            }
        }
    }
}