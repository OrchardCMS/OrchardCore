using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Users.TimeZone.ViewModels;

public class UserTimeZoneViewModel
{
    public string TimeZoneId { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> TimeZones { get; set; } = new List<SelectListItem>();
}
