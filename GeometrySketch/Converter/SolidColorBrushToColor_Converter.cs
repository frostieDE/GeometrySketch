﻿using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace GeometrySketch.Converter
{
    class SolidColorBrushToColor_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush scb = new SolidColorBrush((value is Windows.UI.Color) ? (Windows.UI.Color)value : Colors.Black);
            return scb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
