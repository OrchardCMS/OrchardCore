namespace OrchardCore.ContentManagement.Metadata.Models;

public class ContentTypeDefinitionDriverOptions
{
    public bool ShowCreatable { get; set; } = true;

    public bool ShowListable { get; set; } = true;

    public bool ShowDraftable { get; set; } = true;

    public bool ShowVersionable { get; set; } = true;

    public bool ShowSecurable { get; set; } = true;

    public IEnumerable<StereotypeDescription> Stereotypes { get; set; }
}
