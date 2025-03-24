using Android.Content;
using SeedHealthApp.Custom.Controls;
using SeedHealthApp.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BorderlessPicker), typeof(BorderlessPickerRenderer))]
namespace SeedHealthApp.Droid.Renderers
{
    class BorderlessPickerRenderer : Xamarin.Forms.Platform.Android.AppCompat.PickerRenderer
    {
        public BorderlessPickerRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                Control.Background = null;
            }
        }
    }
}