using System.Windows;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.Services
{
    /// <summary>
    /// 通知表示サービスの実装
    /// </summary>
    public class NotificationService : INotificationService
    {
        private System.Timers.Timer? _infoBarTimer;

        #region INotificationService Implementation

        public bool IsInfoBarOpen { get; private set; }
        public string InfoBarTitle { get; private set; } = string.Empty;
        public string InfoBarMessage { get; private set; } = string.Empty;
        public InfoBarSeverity InfoBarSeverity { get; private set; } = InfoBarSeverity.Informational;

        public event EventHandler<NotificationEventArgs>? NotificationChanged;

        #endregion

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
                IsInfoBarOpen = true;

                // 通知イベントを発火
                NotificationChanged?.Invoke(this, new NotificationEventArgs
                {
                    IsOpen = IsInfoBarOpen,
                    Title = InfoBarTitle,
                    Message = InfoBarMessage,
                    Severity = InfoBarSeverity
                });

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
                            CloseInfoBar();
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
                IsInfoBarOpen = true;

                // 通知イベントを発火
                NotificationChanged?.Invoke(this, new NotificationEventArgs
                {
                    IsOpen = IsInfoBarOpen,
                    Title = InfoBarTitle,
                    Message = InfoBarMessage,
                    Severity = InfoBarSeverity
                });

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
                            CloseInfoBar();
                        });
                    };
                    _infoBarTimer.Start();
                }
            });
        }

        /// <summary>
        /// InfoBarを閉じます
        /// </summary>
        public void CloseInfoBar()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _infoBarTimer?.Dispose();
                _infoBarTimer = null;

                IsInfoBarOpen = false;
                InfoBarTitle = string.Empty;
                InfoBarMessage = string.Empty;
                InfoBarSeverity = InfoBarSeverity.Informational;

                // 通知イベントを発火
                NotificationChanged?.Invoke(this, new NotificationEventArgs
                {
                    IsOpen = IsInfoBarOpen,
                    Title = InfoBarTitle,
                    Message = InfoBarMessage,
                    Severity = InfoBarSeverity
                });
            });
        }
    }
}
