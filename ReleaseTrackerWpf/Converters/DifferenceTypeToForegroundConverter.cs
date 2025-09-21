using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Converters
{
    public class DifferenceTypeToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DifferenceType differenceType)
            {
                return differenceType switch
                {
                    // グレーアウト表示用（DifferenceType.None）
                    DifferenceType.None => new SolidColorBrush(Color.FromRgb(200, 200, 200)), // さらに薄いグレーテキスト

                    // その他は通常のテキスト色（システムのデフォルト色を使用）
                    _ => new SolidColorBrush(Color.FromRgb(0, 0, 0))
                };
            }

            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}