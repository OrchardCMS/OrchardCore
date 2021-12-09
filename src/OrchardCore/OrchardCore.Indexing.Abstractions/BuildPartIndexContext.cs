using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Indexing
{
    public class BuildPartIndexContext : BuildIndexContext
    {
        public BuildPartIndexContext(
            DocumentIndex documentIndex,
            ContentItem contentItem,
            IList<string> keys,
            ContentTypePartDefinition typePartDefinition,
            ContentIndexSettings settings)
            : base(documentIndex, contentItem, keys)
        {
            ContentTypePartDefinition = typePartDefinition;
            Settings = settings;
        }

        public ContentTypePartDefinition ContentTypePartDefinition { get; }
        public ContentIndexSettings Settings { get; }
    }
}
