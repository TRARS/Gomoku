using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gomoku.UI.Control.CustomControlEx.ChatHistoryViewerEx
{
    internal class cChatHistoryViewer_converter_textblock_maxwidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value - 10 - 24 - 10;//10给Margin左右
                                                //24给头像
                                                //10给滚动条    
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class cChatHistoryViewer_converter_textblock_textlimit : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = (string)value;

            if (int.TryParse($"{parameter}", out var size))
            {
                if (text.Length > size)
                {
                    return text.Substring(0, size) + "...";
                }
                else
                {
                    return text;
                }
            }

            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class cChatHistoryViewer_converter_solidcolorbrush2color : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((SolidColorBrush)value).Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class cChatHistoryViewer_converter_imagesource : IValueConverter
    {
        static string? AssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imagePath = $"{value}";
            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                try
                {
                    if (Path.IsPathRooted(imagePath))
                    {
                        // 尝试将路径转换为 ImageSource 对象
                        var imageSource = new BitmapImage(new Uri(imagePath));
                        return imageSource;
                    }
                    else
                    {
                        // 尝试将路径转换为 ImageSource 对象
                        string packUri = $"pack://application:,,,/{AssemblyName};component/{imagePath}";
                        // 尝试将 Pack URI 转换为 ImageSource 对象
                        var imageSource = new BitmapImage(new Uri(packUri));
                        return imageSource;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return new WriteableBitmap(24, 24, 96, 96, PixelFormats.Bgra32, null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class cChatHistoryViewer_converter_actualheight2height : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.Parse($"{value}") > 0)
            {
                return value;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
