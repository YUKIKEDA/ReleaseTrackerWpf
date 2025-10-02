using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace ReleaseTrackerWpf.Behaviors
{
    /// <summary>
    /// ウィンドウを閉じるBehavior
    /// </summary>
    public class WindowCloseBehavior : Behavior<System.Windows.Controls.Button>
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
                window.Close();
            }
        }
    }
}
