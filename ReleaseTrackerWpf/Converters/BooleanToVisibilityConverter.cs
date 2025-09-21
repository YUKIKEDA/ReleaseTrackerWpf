using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ReleaseTrackerWpf.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = parameter?.ToString()?.Equals("Invert", StringComparison.OrdinalIgnoreCase) == true;
            
            if (value is bool boolValue)
            {
                bool result = invert ? !boolValue : boolValue;
                return result ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is string stringValue)
            {
                bool result = invert ? string.IsNullOrEmpty(stringValue) : !string.IsNullOrEmpty(stringValue);
                return result ? Visibility.Visible : Visibility.Collapsed;
            }

            bool nullResult = invert ? value == null : value != null;
            return nullResult ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }

            return false;
        }
    }
}