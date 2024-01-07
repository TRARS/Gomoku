using System;
using System.Globalization;
using System.Windows.Data;

namespace Gomoku.UI.Control.CustomControlEx.ServerStatusViewerEx
{
    internal class cServerStatusViewer_converter_boolinverse : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
