using System.Windows;
using ReleaseTrackerWpf.Services;
using ReleaseTrackerWpf.ViewModels;

namespace ReleaseTrackerWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // サービスの初期化
            var directoryService = new DirectoryService();
            var comparisonService = new ComparisonService();
            var exportService = new ExportService();
            var settingsService = new SettingsService();
            var notificationService = new NotificationService();

            // ViewModelの初期化
            var mainWindowViewModel = new MainWindowViewModel(directoryService, comparisonService, exportService, settingsService, notificationService);

            // MainWindowの設定
            var mainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };

            mainWindow.Show();
        }
    }
}
