using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Workflows.ViewModels;

public class LiquidTaskViewModel
{
    [Required]
    public string Expression { get; set; }
}
