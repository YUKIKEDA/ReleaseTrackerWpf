using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using ReleaseTrackerWpf.Models;
using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.Services
{
    /// <summary>
    /// 通知表示サービスの実装
    /// </summary>
    public class NotificationService : INotificationService
    {
        private System.Timers.Timer? _infoBarTimer;

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

                // Messengerで通知を送信
                WeakReferenceMessenger.Default.Send(new NotificationMessage(
                    true, title, $"🔄 {message}", InfoBarSeverity.Informational));

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

                // Messengerで通知を送信
                WeakReferenceMessenger.Default.Send(new NotificationMessage(
                    true, title, message, severity));

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

                // Messengerで通知を送信
                WeakReferenceMessenger.Default.Send(new NotificationMessage(
                    false, string.Empty, string.Empty, InfoBarSeverity.Informational));
            });
        }
    }
}
