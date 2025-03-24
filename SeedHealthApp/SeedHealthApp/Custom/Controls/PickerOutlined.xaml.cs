using System.Collections;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SeedHealthApp.Custom.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PickerOutlined : ContentView
    {
        public PickerOutlined()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(EntryOutlined), null);

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        public static readonly BindableProperty PlaceholderColorProperty =
            BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(EntryOutlined), Color.Blue);

        public Color PlaceholderColor
        {
            get { return (Color)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }

        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EntryOutlined), Color.Blue);

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(EntryOutlined), Color.Black);

        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public bool IsTitleUp { get; set; }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(EntryOutlined), null);
        public IList ItemsSource
        {
            get { return (IList)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly BindableProperty ItemDisplayBindingProperty =
            BindableProperty.Create(nameof(ItemDisplayBinding), typeof(string), typeof(EntryOutlined), string.Empty);
        public string ItemDisplayBinding
        {
            get { return (string)GetValue(ItemDisplayBindingProperty); }
            set { SetValue(ItemDisplayBindingProperty, value); }
        }
        //BindingBase ItemDisplayBinding
    }
}