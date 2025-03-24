using Prism.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.Forms;

namespace SeedHealthApp.Services
{
    public interface IToastService
    {
        void ShowToast(string title);
    }

    public class ToastService : IToastService
    {
        //IApplicationProvider _applicationProvider;
        //public ToastService(IApplicationProvider applicationProvider)
        //{
        //    _applicationProvider = applicationProvider;
        //}
        public void ShowToast(string title)
        {
            Application.Current.MainPage.DisplayToastAsync(title);
        }
    }
}
