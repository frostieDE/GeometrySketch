using System;
using Windows.UI.Xaml.Data;

namespace GeometrySketch.Converter
{
    public class RadiusToLE_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var b = (Double)value;
            var d = (Decimal)b;
            d = Math.Round(d / 100, 2);
            return d.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
