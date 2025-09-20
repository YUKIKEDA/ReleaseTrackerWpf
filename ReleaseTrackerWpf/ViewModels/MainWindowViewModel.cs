using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReleaseTrackerWpf.Services;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly DirectoryService _directoryService;
        private readonly ComparisonService _comparisonService;
        private readonly ExportService _exportService;

        #region Observable Properties

        // InfoBar関連のプロパティ
        [ObservableProperty]
        private bool isInfoBarOpen = false;

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

        private System.Timers.Timer? _infoBarTimer;

        public MainWindowViewModel(DirectoryService directoryService, ComparisonService comparisonService, ExportService exportService, ISettingsService settingsService)
        {
            _directoryService = directoryService;
            _comparisonService = comparisonService;
            _exportService = exportService;

            // Initialize child ViewModels
            ComparisonViewModel = new ComparisonViewModel(directoryService, comparisonService, exportService);
            SettingsViewModel = new SettingsViewModel(settingsService);

            // Load available snapshots
            _ = LoadAvailableSnapshotsAsync();
        }

        #region Commands

        [RelayCommand]
        private async Task LoadAvailableSnapshotsAsync()
        {
            await ComparisonViewModel.LoadAvailableSnapshotsAsync(SettingsViewModel.SnapshotsDirectory);
        }

        #endregion


        #region Private Methods

        #endregion


        #region Utility Methods

        /// <summary>
        /// プログレス付きInfoBarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        public void ShowProgressInfoBar(string title, string message, int timeoutSeconds = 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 既存のタイマーがあればクリア
                _infoBarTimer?.Dispose();

                // InfoBarの設定（プログレス表示用のメッセージに変更）
                InfoBarTitle = title;
                InfoBarMessage = $"🔄 {message}";
                InfoBarSeverity = InfoBarSeverity.Informational;

                // InfoBarを表示
                IsInfoBarOpen = true;

                // タイムアウトが設定されている場合
                if (timeoutSeconds > 0)
                {
                    _infoBarTimer = new System.Timers.Timer(timeoutSeconds * 1000);
                    _infoBarTimer.Elapsed += (s, e) =>
                    {
                        _infoBarTimer.Stop();
                        _infoBarTimer.Dispose();
                        _infoBarTimer = null;
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsInfoBarOpen = false;
                        });
                    };
                    _infoBarTimer.Start();
                }
            });
        }

        /// <summary>
        /// InfoBarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="severity">InfoBarSeverity</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        public void ShowInfoBar(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational, int timeoutSeconds = 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 既存のタイマーがあればクリア
                _infoBarTimer?.Dispose();

                // InfoBarの設定
                InfoBarTitle = title;
                InfoBarMessage = message;
                InfoBarSeverity = severity;

                // InfoBarを表示
                IsInfoBarOpen = true;

                // タイムアウトが設定されている場合
                if (timeoutSeconds > 0)
                {
                    _infoBarTimer = new System.Timers.Timer(timeoutSeconds * 1000);
                    _infoBarTimer.Elapsed += (s, e) =>
                    {
                        _infoBarTimer.Stop();
                        _infoBarTimer.Dispose();
                        _infoBarTimer = null;
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            IsInfoBarOpen = false;
                        });
                    };
                    _infoBarTimer.Start();
                }
            });
        }

        #endregion
    }
}