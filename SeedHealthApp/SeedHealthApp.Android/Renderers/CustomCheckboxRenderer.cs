using Android.Content;
using Android.Views;
using SeedHealthApp.Custom.Controls;
using SeedHealthApp.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomCheckbox), typeof(CustomCheckboxRenderer))]
namespace SeedHealthApp.Droid.Renderers
{
    public class CustomCheckboxRenderer : CheckBoxRenderer
    {
        public CustomCheckboxRenderer(Context context) : base(context)
        {
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (Element.InputTransparent)
            {
                return false;
            }
            return base.DispatchTouchEvent(e);
        }
    }
}