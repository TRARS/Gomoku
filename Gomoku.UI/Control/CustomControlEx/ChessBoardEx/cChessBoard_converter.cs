using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Gomoku.UI.Control.CustomControlEx.ChessBoardEx
{
    class cChessBoard_converter_boardsize2weekpointmargin : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var ellSize = double.Parse($"{values[0]}"); //5 32 14 14
                var gridSize = double.Parse($"{values[1]}");
                var xCount = double.Parse($"{values[2]}");
                var yCount = double.Parse($"{values[3]}");

                var nkrX = Math.Max((xCount / 2) - 4, 0);
                var nkrY = Math.Max((yCount / 2) - 4, 0);

                // (32*14)/2/(14/2)*3/(5/2)
                var finalX = (gridSize * xCount) / 2 / (xCount / 2) * nkrX - (ellSize / 2);
                var finalY = (gridSize * yCount) / 2 / (yCount / 2) * nkrY - (ellSize / 2);

                return new Thickness(finalX, finalY, 0, 0);
            }
            catch
            {
                return Binding.DoNothing;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class cChessBoard_converter_weekpointmargin2margin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var margin = (Thickness)value;
            return new Thickness(margin.Bottom, margin.Left, margin.Top, margin.Right);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
