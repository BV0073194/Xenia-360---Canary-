using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Xenia_360____Canary_.Converters
{
    public class DoubleToProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return d / 100.0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}