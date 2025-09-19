using System.Windows;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Services;
using ReleaseTrackerWpf.ViewModels;

namespace ReleaseTrackerWpf.Views
{
    public partial class ComparisonWindow : Wpf.Ui.Controls.FluentWindow
    {
        private readonly ComparisonWindowViewModel _viewModel;

        public ComparisonWindow()
        {
            InitializeComponent();

            // 依存性注入がない場合の簡易的な初期化
            var exportService = new ExportService();
            _viewModel = new ComparisonWindowViewModel(exportService);
            DataContext = _viewModel;
        }

        public ComparisonWindow(IExportService exportService)
        {
            InitializeComponent();
            _viewModel = new ComparisonWindowViewModel(exportService);
            DataContext = _viewModel;
        }

        public void LoadComparison(DirectorySnapshot oldSnapshot, DirectorySnapshot newSnapshot, ComparisonResult comparisonResult)
        {
            _viewModel.LoadComparison(oldSnapshot, newSnapshot, comparisonResult);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}