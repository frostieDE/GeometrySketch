using System;
using Windows.UI.Xaml.Data;

namespace GeometrySketch.Converter
{
    public class TrueToFalse_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = (bool)value;
            if (b == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var b = (bool)value;
            if (b == false)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}