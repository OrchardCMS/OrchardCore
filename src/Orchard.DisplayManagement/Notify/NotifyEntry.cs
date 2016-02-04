using Microsoft.AspNet.Mvc.Localization;
using Microsoft.AspNet.Mvc.Rendering;

namespace Orchard.DisplayManagement.Notify
{
    public enum NotifyType
    {
        Success,
        Information,
        Warning,
        Error
    }

    public class NotifyEntry
    {
        public NotifyType Type { get; set; }
        public HtmlString Message { get; set; }
    }
}