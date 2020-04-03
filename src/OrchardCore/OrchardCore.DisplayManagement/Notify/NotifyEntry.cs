using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.DisplayManagement.Notify
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
        public IHtmlContent Message { get; set; }
    }
}