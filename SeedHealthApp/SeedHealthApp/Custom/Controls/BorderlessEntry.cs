using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SeedHealthApp.Custom.Controls
{
    public class BorderlessEntry : Entry
    {
        //public static readonly BindableProperty SetSelectAllOnFocusProperty =
        //BindableProperty.Create("SetSelectAllOnFocus", typeof(bool), typeof(BorderlessEntry), defaultBindingMode: BindingMode.OneWay);

        //public bool SetSelectAllOnFocus
        //{
        //    get { return (bool)GetValue(SetSelectAllOnFocusProperty); }
        //    set { SetValue(SetSelectAllOnFocusProperty, value); }
        //}
        public bool SelectAllOnFocus { get; set; }
    }
}
