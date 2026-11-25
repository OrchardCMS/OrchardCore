using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentTypes.ViewModels;

public class CreatePartViewModel
{
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the editor of the new content part, built by the display drivers of <see cref="CreatePartViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
