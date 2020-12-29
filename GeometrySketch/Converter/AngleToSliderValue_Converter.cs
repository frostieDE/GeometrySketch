using System;
using Windows.UI.Xaml.Data;

namespace GeometrySketch.Converter
{
    class AngleToSliderValue_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var a = (double)value;
            return 360 - a;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var a = (double)value;
            return -a;
        }
    }
}
