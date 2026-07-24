using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Media.ViewModels;

public class MediaApiSettingsViewModel
{
    public MediaApiAuthenticationScheme AuthenticationScheme { get; set; }

    public IEnumerable<SelectListItem> AuthenticationSchemes { get; set; }
}
