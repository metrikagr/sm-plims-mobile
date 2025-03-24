using Xamarin.Forms;

namespace SeedHealthApp.Views
{
    public partial class ElisaDistributionPage : ContentPage
    {
        public ElisaDistributionPage()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                bool result = await DisplayAlert("Warning", "Do you really want to exit?" + System.Environment.NewLine
                    + "Distribution is not saved", "Yes", "No");
                if (result)
                {
                    _ = await Navigation.PopAsync();
                }
            });

            return true;
        }
    }
}
