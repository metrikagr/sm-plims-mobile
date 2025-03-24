using System.Collections.Generic;
using Xamarin.Forms;

namespace SeedHealthApp.Views
{
    public partial class ResultsElisaPage : ContentPage
    {
        public ResultsElisaPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            //MessagingCenter.Subscribe<ViewModels.ResultsElisaPageViewModel, IEnumerable<Models.PlateLocation>>(this, "EvaluationDataGrid.LoadEvaluation",
            //(vm, descriptorList) =>
            //{
            //    if (descriptorList != null)
            //    {
            //        foreach (var item in descriptorList)
            //        {
            //            var btn = GridPlate.FindByName<Button>("btn" + item.Position);
            //            if (btn != null)
            //            {
            //                btn.Text = item.Title;
            //                btn.BackgroundColor = Color.Accent;
            //            }
            //        }
                    
            //    }
            //});
        }
        protected override void OnDisappearing()
        {
            //MessagingCenter.Unsubscribe<ViewModels.ResultsElisaPageViewModel, IEnumerable<Models.PlateLocation>>(this, "EvaluationDataGrid.LoadEvaluation");

            base.OnDisappearing();
        }
    }
}
