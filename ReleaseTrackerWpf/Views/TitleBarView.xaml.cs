using System.Windows;
using System.Windows.Controls;

namespace ReleaseTrackerWpf.Views
{
    /// <summary>
    /// TitleBarView.xaml の相互作用ロジック
    /// </summary>
    public partial class TitleBarView : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(TitleBarView), new PropertyMetadata("ReleaseTracker"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public TitleBarView()
        {
            InitializeComponent();
        }
    }
}
