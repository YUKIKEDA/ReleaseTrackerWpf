using CommunityToolkit.Mvvm.ComponentModel;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Services;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly DirectoryService _directoryService;
        private readonly ComparisonService _comparisonService;
        private readonly ExportService _exportService;
        private readonly INotificationService _notificationService;

        #region Observable Properties

        // InfoBar関連のプロパティ（NotificationServiceから取得）
        public bool IsInfoBarOpen => _notificationService.IsInfoBarOpen;
        public string InfoBarTitle => _notificationService.InfoBarTitle;
        public string InfoBarMessage => _notificationService.InfoBarMessage;
        public InfoBarSeverity InfoBarSeverity => _notificationService.InfoBarSeverity;

        #endregion

        #region Child ViewModels

        public ComparisonViewModel ComparisonViewModel { get; }
        public SettingsViewModel SettingsViewModel { get; }

        #endregion

        public MainWindowViewModel(MainWindowViewModelArgs args)
        {
            _directoryService = args.DirectoryService;
            _comparisonService = args.ComparisonService;
            _exportService = args.ExportService;
            _notificationService = args.NotificationService;

            // Initialize child ViewModels
            var comparisonArgs = new ComparisonViewModelArgs(
                args.DirectoryService, 
                args.ComparisonService, 
                args.ExportService, 
                args.NotificationService, 
                args.SettingsRepository,
                args.SnapshotRepository
            );
            ComparisonViewModel = new ComparisonViewModel(comparisonArgs);

            var settingsArgs = new SettingsViewModelArgs(
                args.SettingsRepository, 
                args.NotificationService
            );
            SettingsViewModel = new SettingsViewModel(settingsArgs);

            // NotificationServiceの変更を監視
            _notificationService.NotificationChanged += OnNotificationChanged;
        }

        private void OnNotificationChanged(object? sender, NotificationEventArgs e)
        {
            // プロパティ変更を通知
            OnPropertyChanged(nameof(IsInfoBarOpen));
            OnPropertyChanged(nameof(InfoBarTitle));
            OnPropertyChanged(nameof(InfoBarMessage));
            OnPropertyChanged(nameof(InfoBarSeverity));
        }

        #region Public Methods

        public async Task LoadSettingsAsync()
        {
            await SettingsViewModel.LoadSettingsAsync();
        }

        public async Task LoadAvailableSnapshotsAsync()
        {
            await ComparisonViewModel.LoadAvailableSnapshotsAsync(SettingsViewModel.SnapshotsDirectory);
        }

        #endregion
    }
}