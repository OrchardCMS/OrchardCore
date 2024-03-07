using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Metadata.Models;

public class ContentTypeDefinitionOptions
{
    public Dictionary<string, ContentTypeDefinitionDriverOptions> Stereotypes { get; } = [];

    public Dictionary<string, ContentTypeDefinitionDriverOptions> ContentTypes { get; } = [];
}
