using SeedHealthApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace SeedHealthApp.Custom.Converters
{
    public class IsNotReadingDataEntryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var readingDataEntry = (ReadingDataEntry)value;
            return readingDataEntry != ReadingDataEntry.Entry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
