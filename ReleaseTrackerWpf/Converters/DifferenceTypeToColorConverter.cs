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
                    // 追加ファイル：左側グレーアウト、右側グリーン背景
                    DifferenceType.Added when side == "left" => new SolidColorBrush(isHover ? Color.FromRgb(220, 220, 220) : Color.FromRgb(240, 240, 240)), // グレーアウト（ホバー時濃く）
                    DifferenceType.Added when side == "right" => new SolidColorBrush(isHover ? Color.FromRgb(210, 255, 210) : Color.FromRgb(230, 255, 230)), // グリーン背景（ホバー時濃く）

                    // 削除ファイル：左側赤背景、右側グレーアウト
                    DifferenceType.Deleted when side == "left" => new SolidColorBrush(isHover ? Color.FromRgb(255, 210, 210) : Color.FromRgb(255, 230, 230)), // 赤背景（ホバー時濃く）
                    DifferenceType.Deleted when side == "right" => new SolidColorBrush(isHover ? Color.FromRgb(220, 220, 220) : Color.FromRgb(240, 240, 240)), // グレーアウト（ホバー時濃く）

                    // 変更ファイル：両方オレンジ背景
                    DifferenceType.Modified => new SolidColorBrush(isHover ? Color.FromRgb(255, 230, 180) : Color.FromRgb(255, 240, 200)), // オレンジ背景（ホバー時濃く）

                    // グレーアウト表示用（DifferenceType.None）
                    DifferenceType.None => new SolidColorBrush(isHover ? Color.FromRgb(220, 220, 220) : Color.FromRgb(240, 240, 240)), // グレーアウト（ホバー時濃く）

                    // 変更なし：透明背景
                    DifferenceType.Unchanged => new SolidColorBrush(Colors.Transparent),

                    // デフォルト
                    _ => new SolidColorBrush(Colors.Transparent)
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