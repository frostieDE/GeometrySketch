using System;
using Windows.UI.Xaml.Data;

namespace GeometrySketch.Converter
{
    public class AngleToMathAngle_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var a = (double)value;
            double b = a % 360;

            if (b > 0)
            {
                b = Math.Round(360 - b, 0);
                return b.ToString();
            }            
            else
            {
                b = Math.Round(-b, 0);
                return b.ToString();
            }           
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }    
}
