using System.Globalization;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.Converters
{
    public class FolderFileIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDirectory)
            {
                // WPF-UIのSymbolIconを使用
                return isDirectory ? SymbolRegular.Folder24 : SymbolRegular.Document24;
            }

            return SymbolRegular.Document24; // デフォルトはファイルアイコン
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
