using SeedHealthApp.Custom.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SeedHealthApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabletAssayPreparationPage : ContentPage
    {
        public TabletAssayPreparationPage()
        {
            InitializeComponent();

            Xamarin.Forms.MessagingCenter.Subscribe<string, int>(this, "LoadDataGrid", (s, activityQty) =>
            {
                //var header_grid = new Grid { Padding = 2, ColumnSpacing = 4, RowSpacing = 1 };
                //datagridHeader.RowDefinitions.Add(new RowDefinition { Height = 35 });

                datagridHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                Label code = new Label { FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center };
                code.Text = "Sample name";
                datagridHeader.Children.Add(code, 0, 0);

                for (int i = 1; i <= activityQty; i++)
                {
                    datagridHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength( 1, GridUnitType.Star) });
                    Label activityNumber = new Label { FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center };
                    activityNumber.Text = "Rep " + i;
                    datagridHeader.Children.Add(activityNumber, i, 0);
                }

                //datagrid.Header = header_grid;

                datagrid.ItemTemplate = new DataTemplate(() =>
                {
                    Grid grid1 = new Grid { Padding = 0, ColumnSpacing = 4, RowSpacing = -1 };
                    grid1.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                    grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    Label label_code = new Label { HorizontalOptions = LayoutOptions.Center, VerticalTextAlignment = TextAlignment.Center};
                    label_code.SetBinding(Label.TextProperty, "composite_sample_name");
                    grid1.Children.Add(label_code, 0, 0);

                    for (int i = 0; i < activityQty; i++)
                    {
                        grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        Frame frame = new Frame()
                        {
                            HasShadow = false,
                            CornerRadius = 4,
                            Padding = new Thickness(5, 0),
                            Margin = 2,
                        };
                        frame.SetBinding(Frame.BorderColorProperty, $"tested_seeds_counts[{i}].BackgroundColor");
                        BorderlessEntry seedsCountEntry = new BorderlessEntry
                        {
                            MaxLength = 3,
                            Keyboard = Keyboard.Numeric,
                            ReturnType = ReturnType.Next,
                            Margin = new Thickness(8, 0),
                            FontSize = 17,
                            HorizontalTextAlignment = TextAlignment.End
                        };
                        seedsCountEntry.SetBinding(Entry.TextProperty, $"tested_seeds_counts[{i}].DisplayValue");
                        //seedsCountEntry.SetBinding(Entry.BackgroundColorProperty, $"tested_seeds_counts[{i}].BackgroundColor");
                        seedsCountEntry.SetBinding(Entry.IsEnabledProperty, $"tested_seeds_counts[{i}].Enabled");
                        
                        grid1.Children.Add(frame, i + 1, 0);
                        grid1.Children.Add(seedsCountEntry, i + 1, 0);
                    }

                    return grid1;
                });
            });
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var header_grid = new Grid { Padding = 2, ColumnSpacing = 1, RowSpacing = 1 };
            header_grid.RowDefinitions.Add(new RowDefinition { Height = 35 });
            header_grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });
            header_grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });
            header_grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });
            header_grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.3, GridUnitType.Star) });
            header_grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });

            Label code = new Label { FontAttributes = FontAttributes.Bold };
            code.Text = "code";
            Label price = new Label { FontAttributes = FontAttributes.Bold };
            price.Text = "price";
            Label quality = new Label { FontAttributes = FontAttributes.Bold };
            quality.Text = "quality";
            Label unit = new Label { FontAttributes = FontAttributes.Bold };
            unit.Text = "unit";
            Label place = new Label { FontAttributes = FontAttributes.Bold };
            place.Text = "place";

            header_grid.Children.Add(code, 0, 0);
            header_grid.Children.Add(price, 1, 0);
            header_grid.Children.Add(quality, 2, 0);
            header_grid.Children.Add(unit, 3, 0);
            header_grid.Children.Add(place, 4, 0);

            datagrid.Header = header_grid;

            datagrid.ItemTemplate = new DataTemplate(() =>
            {
                Grid grid1 = new Grid { Padding = 0, ColumnSpacing = -1, RowSpacing = -1 };
                grid1.RowDefinitions.Add(new RowDefinition { Height = 40 });
                grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });
                grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });
                grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });
                grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.3, GridUnitType.Star) });
                grid1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) });

                Label label_code = new Label { HorizontalOptions = LayoutOptions.Center };
                label_code.SetBinding(Label.TextProperty, "composite_sample_name");

                Label label_price = new Label { HorizontalOptions = LayoutOptions.Center };
                label_price.SetBinding(Label.TextProperty, "tested_seeds_counts[0].DisplayValue");

                Label label_quality = new Label { HorizontalOptions = LayoutOptions.Center };
                label_quality.SetBinding(Label.TextProperty, "tested_seeds_counts[1].DisplayValue");

                Entry label_unit = new Entry { HorizontalOptions = LayoutOptions.Center };
                label_unit.SetBinding(Label.TextProperty, "tested_seeds_counts[2].DisplayValue");

                Entry label_place = new Entry { HorizontalOptions = LayoutOptions.Center };
                label_place.SetBinding(Label.TextProperty, "tested_seeds_counts[3].DisplayValue");

                grid1.Children.Add(label_code, 0, 0);
                grid1.Children.Add(label_price, 1, 0);
                grid1.Children.Add(label_quality, 2, 0);
                grid1.Children.Add(label_unit, 3, 0);
                grid1.Children.Add(label_place, 4, 0);

                return grid1;
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            
            //Xamarin.Forms.MessagingCenter.Send<string, int>(null, "LoadDataGrid", (int)SelectedSampleType.activity_qty);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            Xamarin.Forms.MessagingCenter.Unsubscribe<string, int>(this, "LoadDataGrid");
        }
    }
}