using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReleaseTrackerWpf.Views
{
    /// <summary>
    /// ComparisonView.xaml の相互作用ロジック
    /// </summary>
    public partial class ComparisonView : UserControl
    {
        private bool _isSyncing = false; // 無限ループ防止フラグ

        public ComparisonView()
        {
            InitializeComponent();
        }

        private void ComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // ComboBoxのマウスホイールによる選択変更を無効化
            e.Handled = true;
        }

        private ScrollViewer? GetScrollViewer(DependencyObject? obj)
        {
            if (obj == null) return null;

            if (obj is ScrollViewer scrollViewer)
                return scrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var listView = sender as ListView;
            if (listView == null) return;

            var scrollViewer = GetScrollViewer(listView);
            if (scrollViewer == null) return;

            // Shiftキーが押されている場合は横スクロール
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (e.Delta > 0)
                {
                    // 上方向のスクロール = 左スクロール
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 50);
                }
                else
                {
                    // 下方向のスクロール = 右スクロール
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + 50);
                }
                e.Handled = true;
            }
            // Shiftキーが押されていない場合は通常の縦スクロール（デフォルト動作）
        }

        private void LeftScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isSyncing) return;

            _isSyncing = true;
            try
            {
                // 左側のスクロール変更を右側に同期
                var rightScrollViewer = GetScrollViewer(RightListView);
                if (rightScrollViewer != null)
                {
                    rightScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
                    rightScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private void RightScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_isSyncing) return;

            _isSyncing = true;
            try
            {
                // 右側のスクロール変更を左側に同期
                var leftScrollViewer = GetScrollViewer(LeftListView);
                if (leftScrollViewer != null)
                {
                    leftScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
                    leftScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}
