using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement.MetaData.Models
{
    public class ContentTypePartDefinition
    {
        public ContentTypePartDefinition(ContentPartDefinition contentPartDefinition, JObject settings)
        {
            PartDefinition = contentPartDefinition;
            Settings = settings;
        }

        public ContentTypePartDefinition()
        {
            Settings = new JObject();
        }

        public ContentPartDefinition PartDefinition { get; private set; }
        public JObject Settings { get; private set; }
        public ContentTypeDefinition ContentTypeDefinition { get; set; }
    }
}