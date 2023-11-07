using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Forms.Activities.ViewModels;

public class HttpRedirectToFormLocationTaskViewModel
{
    [Required]
    public string FormLocationKey { get; set; }
}
