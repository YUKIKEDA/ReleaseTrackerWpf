using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using ReleaseTrackerWpf.Services;
using ReleaseTrackerWpf.ViewModels;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf
{
    public partial class MainWindow : FluentWindow
    {

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services
            var directoryService = new DirectoryService();
            var comparisonService = new ComparisonService();
            var exportService = new ExportService();

            // Set DataContext
            var viewModel = new MainViewModel(directoryService, comparisonService, exportService);
            DataContext = viewModel;
        }

        private void OpenSnapshotsFolder_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null && Directory.Exists(viewModel.SnapshotsDirectory))
            {
                Process.Start("explorer.exe", viewModel.SnapshotsDirectory);
            }
        }

        private void ChangeSnapshotsFolder_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                var dialog = new OpenFileDialog
                {
                    Title = "スナップショット保存先フォルダを選択",
                    FileName = "フォルダを選択",
                    Filter = "Folder|*.folder",
                    CheckFileExists = false,
                    CheckPathExists = true
                };

                var result = dialog.ShowDialog();
                if (result == true)
                {
                    var selectedPath = Path.GetDirectoryName(dialog.FileName);
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        viewModel.SnapshotsDirectory = selectedPath;
                    }
                }
            }
        }
    }
}