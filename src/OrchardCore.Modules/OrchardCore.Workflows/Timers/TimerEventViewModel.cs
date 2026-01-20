using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.Timers;

public class TimerEventViewModel
{
    [Required]
    public string CronExpression { get; set; }

    public bool UseLocalTime { get; set; }
}
