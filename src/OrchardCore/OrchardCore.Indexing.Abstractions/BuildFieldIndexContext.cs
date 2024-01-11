using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Indexing
{
    public class BuildFieldIndexContext : BuildPartIndexContext
    {
        public BuildFieldIndexContext(
            DocumentIndex documentIndex,
            ContentItem contentItem,
            IList<string> keys,
            ContentPart contentPart,
            ContentTypePartDefinition typePartDefinition,
            ContentPartFieldDefinition partFieldDefinition,
            IContentIndexSettings settings)
            : base(documentIndex, contentItem, keys, typePartDefinition, settings)
        {
            ContentPartFieldDefinition = partFieldDefinition;
            ContentPart = contentPart;
        }

        public ContentPart ContentPart { get; }
        public ContentPartFieldDefinition ContentPartFieldDefinition { get; }
    }
}
