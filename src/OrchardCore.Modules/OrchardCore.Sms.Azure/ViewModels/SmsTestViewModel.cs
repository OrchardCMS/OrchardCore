using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Sms.Azure.ViewModels;

public class SmsTestViewModel
{
    [Required]
    public string Provider { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

    [BindNever]
    public IList<SelectListItem> Providers { get; set; }
}
