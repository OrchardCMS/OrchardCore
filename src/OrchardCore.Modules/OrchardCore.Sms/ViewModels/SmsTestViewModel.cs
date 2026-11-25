using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Sms.ViewModels;

public class SmsTestViewModel
{
    [Required]
    public string Provider { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

    [BindNever]
    public IList<SelectListItem> Providers { get; set; }

    /// <summary>
    /// Gets or sets the editor of the test SMS page, built by the display drivers of <see cref="SmsTestViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
