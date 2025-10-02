using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReleaseTrackerWpf.Converters
{
    public class DepthToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int depth)
            {
                // 各レベルごとに20ピクセルのインデントを追加
                var indentSize = 20;
                var leftMargin = depth * indentSize;
                return new Thickness(leftMargin, 0, 0, 0);
            }

            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
