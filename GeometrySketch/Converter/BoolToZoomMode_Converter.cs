using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace GeometrySketch.Converter
{
    public class BoolToZoomMode_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = (bool)value;
            if (b == false)
            {
                return ZoomMode.Disabled;
            }
            else
            {
                return ZoomMode.Enabled;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var v = (ZoomMode)value;
            if (v == ZoomMode.Disabled)
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