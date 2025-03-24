using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SeedHealthApp.Custom.Controls;
using SeedHealthApp.Droid.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(BorderlessEntryRenderer))]
namespace SeedHealthApp.Droid.Renderers
{
    public class BorderlessEntryRenderer : EntryRenderer
    {
        public BorderlessEntryRenderer(Context context) : base(context)
        {
        }
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            //Configure native control (TextBox)
            if (Control != null)
            {
                Control.Background = null;
            }

            // Configure Entry properties
            if (e.NewElement != null)
            {
            }

            if (Control != null && e.NewElement != null)
            {
                BorderlessEntry customEntry = e.NewElement as BorderlessEntry;
                Control.SetSelectAllOnFocus(customEntry.SelectAllOnFocus);
            }
        }
    }
}