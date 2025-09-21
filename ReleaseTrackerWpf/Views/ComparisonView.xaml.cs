using System.Windows.Controls;
using System.Windows.Input;

namespace ReleaseTrackerWpf.Views
{
    /// <summary>
    /// ComparisonView.xaml の相互作用ロジック
    /// </summary>
    public partial class ComparisonView : UserControl
    {
        public ComparisonView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
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
        }
    }
}
