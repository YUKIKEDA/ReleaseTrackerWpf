using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.Services
{
    /// <summary>
    /// 通知表示サービス用のインターフェース
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// InfoBarが開いているかどうか
        /// </summary>
        bool IsInfoBarOpen { get; }

        /// <summary>
        /// InfoBarのタイトル
        /// </summary>
        string InfoBarTitle { get; }

        /// <summary>
        /// InfoBarのメッセージ
        /// </summary>
        string InfoBarMessage { get; }

        /// <summary>
        /// InfoBarの重要度
        /// </summary>
        InfoBarSeverity InfoBarSeverity { get; }

        /// <summary>
        /// InfoBarの状態が変更された時のイベント
        /// </summary>
        event EventHandler<NotificationEventArgs>? NotificationChanged;

        /// <summary>
        /// プログレス付きInfoBarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        void ShowProgressInfoBar(string title, string message, int timeoutSeconds = 0);

        /// <summary>
        /// InfoBarを表示します
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="message">メッセージ</param>
        /// <param name="severity">InfoBarSeverity</param>
        /// <param name="timeoutSeconds">表示時間（秒、0で無制限）</param>
        void ShowInfoBar(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational, int timeoutSeconds = 0);

        /// <summary>
        /// InfoBarを閉じます
        /// </summary>
        void CloseInfoBar();
    }

    /// <summary>
    /// 通知イベントの引数
    /// </summary>
    public class NotificationEventArgs : EventArgs
    {
        public bool IsOpen { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Informational;
    }
}
