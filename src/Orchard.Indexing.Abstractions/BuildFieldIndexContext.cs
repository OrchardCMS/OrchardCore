using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.Indexing
{
    public class BuildFieldIndexContext : BuildPartIndexContext
    {
        public BuildFieldIndexContext(
            DocumentIndex documentIndex, 
            ContentItem contentItem, 
            string key, 
            ContentPart contentPart, 
            ContentTypePartDefinition typePartDefinition, 
            ContentPartFieldDefinition partFieldDefinition, 
            ContentIndexSettings settings)
            :base(documentIndex, contentItem, key, typePartDefinition, settings)
        {
            ContentPartFieldDefinition = partFieldDefinition;
            ContentPart = contentPart;
        }

        public ContentPart ContentPart { get; }
        public ContentPartFieldDefinition ContentPartFieldDefinition { get; }
    }
}
