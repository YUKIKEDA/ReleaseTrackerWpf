using Wpf.Ui.Controls;

namespace ReleaseTrackerWpf.Models
{
    /// <summary>
    /// 通知メッセージ
    /// </summary>
    public class NotificationMessage
    {
        public bool IsOpen { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Informational;

        public NotificationMessage(bool isOpen, string title, string message, InfoBarSeverity severity)
        {
            IsOpen = isOpen;
            Title = title;
            Message = message;
            Severity = severity;
        }
    }
}
