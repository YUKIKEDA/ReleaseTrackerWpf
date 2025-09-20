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
                var param = parameter?.ToString()?.ToLower();
                var isHover = param?.Contains("hover") == true;
                var side = param?.Replace("hover", "").Trim();
                
                return differenceType switch
                {
                    DifferenceType.Added when side == "left" => new SolidColorBrush(isHover ? Color.FromRgb(250, 250, 250) : Color.FromRgb(244, 244, 244)), // Hover: very light gray, Normal: light gray
                    DifferenceType.Added when side == "right" => new SolidColorBrush(isHover ? Color.FromRgb(240, 255, 240) : Color.FromRgb(215, 255, 215)), // Hover: very light green, Normal: medium light green
                    DifferenceType.Deleted when side == "left" => new SolidColorBrush(isHover ? Color.FromRgb(255, 240, 240) : Color.FromRgb(255, 215, 215)), // Hover: very light red, Normal: medium light red
                    DifferenceType.Deleted when side == "right" => new SolidColorBrush(isHover ? Color.FromRgb(250, 250, 250) : Color.FromRgb(244, 244, 244)), // Hover: very light gray, Normal: light gray
                    DifferenceType.Modified => new SolidColorBrush(isHover ? Color.FromRgb(255, 250, 240) : Color.FromRgb(255, 240, 215)), // Hover: very light beige, Normal: medium light beige
                    DifferenceType.Unchanged => new SolidColorBrush(Colors.Transparent), // Transparent background for unchanged
                    DifferenceType.Added => new SolidColorBrush(isHover ? Color.FromRgb(240, 255, 240) : Color.FromRgb(215, 255, 215)),     // Default
                    DifferenceType.Deleted => new SolidColorBrush(isHover ? Color.FromRgb(255, 240, 240) : Color.FromRgb(255, 215, 215)),  // Default
                    _ => new SolidColorBrush(Colors.Transparent)                         // Default transparent
                };
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}