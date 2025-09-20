using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ReleaseTrackerWpf.Models;

namespace ReleaseTrackerWpf.Converters
{
    public class DifferenceTypeToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DifferenceType differenceType)
            {
                return differenceType switch
                {
                    DifferenceType.Added => new SolidColorBrush(Color.FromRgb(230, 255, 237)),   // Light green
                    DifferenceType.Deleted => new SolidColorBrush(Color.FromRgb(255, 238, 240)), // Light red
                    DifferenceType.Modified => new SolidColorBrush(Color.FromRgb(255, 248, 225)), // Light orange
                    _ => Brushes.Transparent
                };
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}