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

    }
}