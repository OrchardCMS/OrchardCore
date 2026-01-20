using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Email.ViewModels;

public class EmailTestViewModel
{
    [Required]
    public string Provider { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string To { get; set; }

    [EmailAddress(ErrorMessage = "Invalid Email.")]
    public string From { get; set; }

    public string Bcc { get; set; }

    public string Cc { get; set; }

    public string ReplyTo { get; set; }

    [Required]
    public string Subject { get; set; }

    [Required]
    public string Body { get; set; }

    [BindNever]
    public IList<SelectListItem> Providers { get; set; }
}
