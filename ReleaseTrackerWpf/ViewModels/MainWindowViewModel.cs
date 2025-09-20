using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ReleaseTrackerWpf.Models;
using ReleaseTrackerWpf.Services;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject, IRecipient<NotificationMessage>
    {
        private readonly DirectoryScanService _directoryScanService;
        private readonly ComparisonService _comparisonService;
        private readonly ExportService _exportService;
        private readonly INotificationService _notificationService;

        #region Observable Properties

        [ObservableProperty]
        private bool isInfoBarOpen;

        [ObservableProperty]
        private string infoBarTitle = string.Empty;

        [ObservableProperty]
        private string infoBarMessage = string.Empty;

        [ObservableProperty]
        private InfoBarSeverity infoBarSeverity = InfoBarSeverity.Informational;

        #endregion

        #region Child ViewModels

        public ComparisonViewModel ComparisonViewModel { get; }
        public SettingsViewModel SettingsViewModel { get; }

        #endregion

        public MainWindowViewModel(MainWindowViewModelArgs args)
        {
            _directoryScanService = args.DirectoryScanService;
            _comparisonService = args.ComparisonService;
            _exportService = args.ExportService;
            _notificationService = args.NotificationService;

            // Initialize child ViewModels
            var comparisonArgs = new ComparisonViewModelArgs(
                args.DirectoryScanService, 
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

            // Messengerに登録
            WeakReferenceMessenger.Default.Register<NotificationMessage>(this);
        }

        public void Receive(NotificationMessage message)
        {
            IsInfoBarOpen = message.IsOpen;
            InfoBarTitle = message.Title;
            InfoBarMessage = message.Message;
            InfoBarSeverity = message.Severity;
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