using System;
using System.Globalization;
using System.Windows.Data;

namespace Gomoku.UI.Control.CustomControlEx.ChatInputBoxEx
{
    class cChatInputBox_converter_textlimit : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = (string)value;
            if (text.Length >= 64)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
