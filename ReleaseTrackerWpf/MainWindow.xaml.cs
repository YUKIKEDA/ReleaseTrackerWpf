using Microsoft.Win32;
using ReleaseTrackerWpf.Services;
using ReleaseTrackerWpf.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf
{
    public partial class MainWindow : FluentWindow
    {
        private Timer? _infoBarTimer;

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
            
            // MainWindowの参照をViewModelに設定
            viewModel.SetMainWindow(this);
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


        /// <summary>
        /// InfoBarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        public void ShowSnackbar(string title, string message, int timeoutSeconds = 0)
        {
            System.Diagnostics.Debug.WriteLine($"ShowInfoBar called: Title='{title}', Message='{message}', Timeout={timeoutSeconds}");

            Dispatcher.Invoke(() =>
            {
                // 既存のタイマーがあればクリア
                _infoBarTimer?.Dispose();

                // InfoBarの設定
                NotificationInfoBar.Title = title;
                NotificationInfoBar.Message = message;

                // メッセージ内容に応じてSeverityを設定
                if (message.Contains("エラー") || title.Contains("エラー"))
                {
                    NotificationInfoBar.Severity = InfoBarSeverity.Error;
                }
                else if (message.Contains("完了") || title.Contains("完了") || title.Contains("通知"))
                {
                    NotificationInfoBar.Severity = InfoBarSeverity.Success;
                }
                else if (message.Contains("処理中") || title.Contains("処理中"))
                {
                    NotificationInfoBar.Severity = InfoBarSeverity.Informational;
                }
                else
                {
                    NotificationInfoBar.Severity = InfoBarSeverity.Informational;
                }

                // InfoBarを表示
                NotificationInfoBar.IsOpen = true;

                // タイムアウトが設定されている場合
                if (timeoutSeconds > 0)
                {
                    _infoBarTimer = new Timer(_ =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            NotificationInfoBar.IsOpen = false;
                        });
                        _infoBarTimer?.Dispose();
                        _infoBarTimer = null;
                    }, null, TimeSpan.FromSeconds(timeoutSeconds), Timeout.InfiniteTimeSpan);
                }
            });
        }

        /// <summary>
        /// プログレス付きInfoBarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        public void ShowProgressSnackbar(string title, string message, int timeoutSeconds = 0)
        {
            System.Diagnostics.Debug.WriteLine($"ShowProgressInfoBar called: Title='{title}', Message='{message}', Timeout={timeoutSeconds}");

            Dispatcher.Invoke(() =>
            {
                // 既存のタイマーがあればクリア
                _infoBarTimer?.Dispose();

                // InfoBarの設定（プログレス表示用のメッセージに変更）
                NotificationInfoBar.Title = title;
                NotificationInfoBar.Message = $"🔄 {message}";
                NotificationInfoBar.Severity = InfoBarSeverity.Informational;

                // InfoBarを表示
                NotificationInfoBar.IsOpen = true;

                // タイムアウトが設定されている場合
                if (timeoutSeconds > 0)
                {
                    _infoBarTimer = new Timer(_ =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            NotificationInfoBar.IsOpen = false;
                        });
                        _infoBarTimer?.Dispose();
                        _infoBarTimer = null;
                    }, null, TimeSpan.FromSeconds(timeoutSeconds), Timeout.InfiniteTimeSpan);
                }
            });
        }

        /// <summary>
        /// 現在のInfoBarを閉じます
        /// </summary>
        public void ClearSnackbar()
        {
            Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine("Clearing current InfoBar");
                _infoBarTimer?.Dispose();
                _infoBarTimer = null;
                NotificationInfoBar.IsOpen = false;
            });
        }


    }
}