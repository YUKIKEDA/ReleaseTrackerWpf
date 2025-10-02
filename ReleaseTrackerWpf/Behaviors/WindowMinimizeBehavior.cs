using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace ReleaseTrackerWpf.Behaviors
{
    /// <summary>
    /// ウィンドウを最小化するBehavior
    /// </summary>
    public class WindowMinimizeBehavior : Behavior<System.Windows.Controls.Button>
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
                window.WindowState = WindowState.Minimized;
            }
        }
    }
}
