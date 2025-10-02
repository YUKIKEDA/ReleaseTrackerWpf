using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace ReleaseTrackerWpf.Behaviors
{
    /// <summary>
    /// ウィンドウを最大化/復元するBehavior
    /// </summary>
    public class WindowMaximizeBehavior : Behavior<System.Windows.Controls.Button>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OnClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Click -= OnClick;
        }

        private void OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var window = Window.GetWindow(AssociatedObject);
            if (window != null)
            {
                window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
        }
    }
}
