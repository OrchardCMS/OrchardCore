using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.BackgroundTasks.ViewModels;

public class AdminIndexOptions
{
    public string Search { get; set; }

    public string Status { get; set; }
    public List<SelectListItem> Statuses { get; internal set; }
}
