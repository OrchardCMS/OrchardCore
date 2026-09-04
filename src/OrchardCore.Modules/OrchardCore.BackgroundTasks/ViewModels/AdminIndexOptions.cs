using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.BackgroundTasks.ViewModels;

public class AdminIndexOptions
{
    public string Search { get; set; }

    public string Status { get; set; }
    public List<SelectListItem> Statuses { get; internal set; }

    public BackgroundTaskBulkAction BulkAction { get; set; }

    public List<SelectListItem> BulkActions { get; internal set; }
}
