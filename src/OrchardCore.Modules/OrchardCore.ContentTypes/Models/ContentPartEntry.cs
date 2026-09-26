using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes.Models;

/// <summary>
/// One row of the content parts admin list. Its name is the shape type of the rows built by
/// <see cref="Drivers.ContentPartEntryDisplayDriver"/>, so themes override them with
/// <c>ContentPartEntry-SummaryAdmin.cshtml</c>.
/// </summary>
public class ContentPartEntry
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string Description { get; set; }

    public ContentPartDefinition PartDefinition { get; set; }
}
