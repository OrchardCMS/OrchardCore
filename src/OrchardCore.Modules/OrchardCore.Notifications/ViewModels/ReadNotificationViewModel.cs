using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Notifications.ViewModels;

public class ReadNotificationViewModel
{
    [Required]
    public string MessageId { get; set; }
}
