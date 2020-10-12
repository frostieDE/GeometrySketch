using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GeometrySketch.Converter
{
    public class BoolToVisibility_Converter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = (bool)value;
            if (b == false)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var v = (Visibility)value;
            if (v == Visibility.Collapsed)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
