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
        /// Snackbarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        public void ShowSnackbar(string title, string message, int timeoutSeconds = 0)
        {
            System.Diagnostics.Debug.WriteLine($"ShowSnackbar called: Title='{title}', Message='{message}', Timeout={timeoutSeconds}");
            System.Diagnostics.Debug.WriteLine($"SnackbarPresenter is null: {SnackbarPresenter == null}");
            
            if (SnackbarPresenter == null)
            {
                System.Diagnostics.Debug.WriteLine("SnackbarPresenter is null, cannot show snackbar");
                return;
            }
            
            var snackbar = new Snackbar(SnackbarPresenter)
            {
                Title = title,
                Content = message,
                IsCloseButtonEnabled = true
            };

            // timeoutSecondsが0でない場合のみTimeoutを設定
            if (timeoutSeconds > 0)
            {
                snackbar.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            }
            // timeoutSecondsが0の場合はデフォルト値を使用（Timeoutを設定しない）

            System.Diagnostics.Debug.WriteLine($"Snackbar created, calling Show()");
            snackbar.Show();
            System.Diagnostics.Debug.WriteLine($"Snackbar.Show() completed");
        }

        /// <summary>
        /// プログレス付きSnackbarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        public void ShowProgressSnackbar(string title, string message, int timeoutSeconds = 0)
        {
            System.Diagnostics.Debug.WriteLine($"ShowProgressSnackbar called: Title='{title}', Message='{message}', Timeout={timeoutSeconds}");
            System.Diagnostics.Debug.WriteLine($"SnackbarPresenter is null: {SnackbarPresenter == null}");
            
            if (SnackbarPresenter == null)
            {
                System.Diagnostics.Debug.WriteLine("SnackbarPresenter is null, cannot show progress snackbar");
                return;
            }
            
            try
            {
                var progressPanel = new StackPanel { Orientation = Orientation.Horizontal };
                var progressRing = new ProgressRing
                {
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(0, 0, 8, 0),
                    IsIndeterminate = true
                };
                progressPanel.Children.Add(progressRing);
                progressPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = message });

                var snackbar = new Snackbar(SnackbarPresenter)
                {
                    Title = title,
                    Content = progressPanel,
                    IsCloseButtonEnabled = true
                };

                // timeoutSecondsが0でない場合のみTimeoutを設定
                if (timeoutSeconds > 0)
                {
                    snackbar.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                }
                // timeoutSecondsが0の場合はデフォルト値を使用（Timeoutを設定しない）

                System.Diagnostics.Debug.WriteLine($"ProgressSnackbar created, calling Show()");
                snackbar.Show();
                System.Diagnostics.Debug.WriteLine($"ProgressSnackbar.Show() completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProgressSnackbar failed: {ex.Message}");
                // ProgressRingでエラーが発生した場合は通常のSnackbarにフォールバック
                ShowSnackbar(title, $"🔄 {message}", timeoutSeconds);
            }
        }


    }
}