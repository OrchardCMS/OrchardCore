using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.Indexing
{
    public class BuildPartIndexContext : BuildIndexContext
    {
        public BuildPartIndexContext(
            DocumentIndex documentIndex, 
            ContentItem contentItem, 
            string key, 
            ContentTypePartDefinition typePartDefinition, 
            ContentIndexSettings settings)
            :base(documentIndex, contentItem, key)
        {
            ContentTypePartDefinition = typePartDefinition;
            Settings = settings;
        }

        public ContentTypePartDefinition ContentTypePartDefinition { get; }
        public ContentIndexSettings Settings { get; }
    }
}
