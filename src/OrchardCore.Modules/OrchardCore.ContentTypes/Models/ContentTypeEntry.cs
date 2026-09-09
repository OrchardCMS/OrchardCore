using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes.Models;

/// <summary>
/// One row of the content types admin list. Its name is the shape type of the rows built by
/// <see cref="Drivers.ContentTypeEntryDisplayDriver"/>, so themes override them with
/// <c>ContentTypeEntry-SummaryAdmin.cshtml</c>.
/// </summary>
public class ContentTypeEntry
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public ContentTypeDefinition TypeDefinition { get; set; }
}
