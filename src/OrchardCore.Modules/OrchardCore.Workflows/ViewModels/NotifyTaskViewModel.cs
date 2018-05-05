using System.ComponentModel.DataAnnotations;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.Workflows.ViewModels
{
    public class NotifyTaskViewModel
    {
        public NotifyType NotificationType { get; set; }

        [Required]
        public string Message { get; set; }
    }
}
