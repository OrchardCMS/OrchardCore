using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Workflows.ViewModels
{
    public class LogTaskViewModel
    {
        public LogLevel LogLevel { get; set; }

        [Required]
        public string Text { get; set; }
    }
}
