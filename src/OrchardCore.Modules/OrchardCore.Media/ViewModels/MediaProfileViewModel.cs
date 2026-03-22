using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Media.Core.Processing;

namespace OrchardCore.Media.ViewModels;

public class MediaProfileViewModel
{
    public string Name { get; set; }
    public string Hint { get; set; }

    public int SelectedWidth { get; set; }
    public int CustomWidth { get; set; }
    public int SelectedHeight { get; set; }
    public int CustomHeight { get; set; }
    public ResizeMode SelectedMode { get; set; }
    public Format SelectedFormat { get; set; }
    public int Quality { get; set; } = 100;
    public string BackgroundColor { get; set; }

    [BindNever]
    public List<SelectListItem> AvailableWidths { get; set; } = [];

    [BindNever]
    public List<SelectListItem> AvailableHeights { get; set; } = [];

    [BindNever]
    public List<SelectListItem> AvailableResizeModes { get; set; } = [];
    [BindNever]
    public List<SelectListItem> AvailableFormats { get; set; } = [];
}
