using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Notifications.ViewModels;

public class NotifyUserTaskActivityViewModel
{
    [Required]
    public string Subject { get; set; }

    public string TextBody { get; set; }

    public string HtmlBody { get; set; }

    public bool IsHtmlPreferred { get; set; }
}
