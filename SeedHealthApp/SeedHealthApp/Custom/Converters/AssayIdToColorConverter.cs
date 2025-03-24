using SeedHealthApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace SeedHealthApp.Custom.Converters
{
    public class AssayIdToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int assayId = (int)value;
            switch (assayId)
            {
                case (int)AssayMobileEnum.Elisa:
                    return "#F9E79F";
                case (int)AssayMobileEnum.Pcr:
                    return "#BCAAA4";
                case (int)AssayMobileEnum.GerminationTest:
                    return "#F5CBA7";
                case (int)AssayMobileEnum.BlotterAndFreezePaperTest:
                    return "#AED6F1";
                default:
                    throw new Exception("Not supported assay");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
