using System.Windows;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Repositories;
using ReleaseTrackerWpf.Services;
using ReleaseTrackerWpf.ViewModels;

namespace ReleaseTrackerWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // リポジトリの初期化
            var settingsRepository = new SettingsRepository();
            var snapshotRepository = new SnapshotRepository();

            // サービスの初期化
            var directoryScanService = new DirectoryScanService();
            var comparisonService = new ComparisonService();
            var exportService = new ExportService();
            var comparisonExportService = new ComparisonExportService();
            var importDescriptionService = new ImportDescriptionService();
            var notificationService = new NotificationService();

            // DTOクラスで依存関係をまとめる
            var args = new MainWindowViewModelArgs(
                directoryScanService,
                comparisonService,
                exportService,
                comparisonExportService,
                importDescriptionService,
                settingsRepository,
                notificationService,
                snapshotRepository
            );

            // ViewModelの初期化
            var mainWindowViewModel = new MainWindowViewModel(args);

            // 設定を読み込み
            await mainWindowViewModel.LoadSettingsAsync();

            // スナップショットを読み込み
            await mainWindowViewModel.LoadAvailableSnapshotsAsync();

            // MainWindowの設定
            var mainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };

            mainWindow.Show();
        }
    }
}
