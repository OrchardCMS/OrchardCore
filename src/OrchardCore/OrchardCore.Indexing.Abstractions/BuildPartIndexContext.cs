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
            IContentIndexSettings settings)
            : base(documentIndex, contentItem, keys, settings)
        {
            ContentTypePartDefinition = typePartDefinition;
        }

        public ContentTypePartDefinition ContentTypePartDefinition { get; }
    }
}
