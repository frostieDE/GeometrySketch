using System;
using Windows.UI.Xaml.Data;

namespace GeometrySketch.Converter
{
    public class EraserWidthToRectangle_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double d;
            int i = (int)value;
            d = (double)i;
            return 2 * d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
