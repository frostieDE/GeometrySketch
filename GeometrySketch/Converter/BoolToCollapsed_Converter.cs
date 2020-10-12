using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace GeometrySketch.Converter
{
    public class BoolToCollapsed_Converter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = (bool)value;
            if (b == false)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var v = (Visibility)value;
            if (v == Visibility.Collapsed)
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
