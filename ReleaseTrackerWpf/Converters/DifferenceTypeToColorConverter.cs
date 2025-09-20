using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Converters
{
    public class DifferenceTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DifferenceType differenceType)
            {
                return differenceType switch
                {
                    DifferenceType.Added => new SolidColorBrush(Color.FromRgb(45, 125, 50)),     // Desert theme green (#2D7D32)
                    DifferenceType.Deleted => new SolidColorBrush(Color.FromRgb(211, 47, 47)),  // Desert theme red (#D32F2F)
                    DifferenceType.Modified => new SolidColorBrush(Color.FromRgb(245, 124, 0)), // Desert theme orange (#F57C00)
                    _ => new SolidColorBrush(Color.FromRgb(61, 61, 61))                         // Desert theme text color (#3D3D3D)
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