using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.Timers
{
    public class TimerEventViewModel
    {
        [Required]
        public string CronExpression { get; set; }
    }
}
