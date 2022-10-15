using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Notifications.ViewModels;

public class ReadWebNotificationViewModel
{
    [Required]
    public string MessageId { get; set; }
}
