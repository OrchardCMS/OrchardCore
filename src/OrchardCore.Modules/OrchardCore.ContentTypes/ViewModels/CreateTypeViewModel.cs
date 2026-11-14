using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.ContentTypes.ViewModels;

public class CreateTypeViewModel
{
    public string DisplayName { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the editor of the new content type, built by the display drivers of <see cref="CreateTypeViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
