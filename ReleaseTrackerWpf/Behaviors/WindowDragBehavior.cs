using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace ReleaseTrackerWpf.Behaviors
{
    /// <summary>
    /// ウィンドウをドラッグできるようにするBehavior
    /// </summary>
    public class WindowDragBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = Window.GetWindow(AssociatedObject);
            if (window != null)
            {
                // ダブルクリックの場合は最大化/復元を切り替え
                if (e.ClickCount == 2)
                {
                    window.WindowState = window.WindowState == WindowState.Maximized 
                        ? WindowState.Normal 
                        : WindowState.Maximized;
                }
                else
                {
                    // シングルクリックの場合はドラッグ開始
                    window.DragMove();
                }
            }
        }
    }
}
