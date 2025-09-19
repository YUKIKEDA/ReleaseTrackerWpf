using ReleaseTrackerWpf.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

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
                    DifferenceType.Added => new SolidColorBrush(Color.FromRgb(0, 120, 0)),      // Darker green
                    DifferenceType.Deleted => new SolidColorBrush(Color.FromRgb(180, 0, 0)),    // Darker red
                    DifferenceType.Modified => new SolidColorBrush(Color.FromRgb(200, 100, 0)), // Darker orange
                    _ => new SolidColorBrush(Colors.Black)
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