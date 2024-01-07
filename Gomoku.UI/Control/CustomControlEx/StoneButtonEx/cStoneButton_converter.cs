using System;
using System.Globalization;
using System.Windows.Data;

namespace Gomoku.UI.Control.CustomControlEx.StoneButtonEx
{
    class cStoneButton_converter_diameter2radius : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
