using System;
using Windows.UI.Xaml.Data;

namespace GeometrySketch.Converter
{
    class SliderValueToRoundedString_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var sv = (double)value;
            sv = Math.Round(sv, 0);
            /*if (sv == 360)
            {
                return "0";
            }
            else
            {
                return sv.ToString();
            }*/

            return sv.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
