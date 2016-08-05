using Newtonsoft.Json.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.MetaData.Builders
{
    public abstract class ContentTypePartDefinitionBuilder
    {
        protected readonly JObject _settings;

        public ContentTypePartDefinition Current { get; protected set; }
        public string Name { get; private set; }
        public string PartName { get; private set; }
        public string TypeName { get; private set; }

        protected ContentTypePartDefinitionBuilder(ContentTypePartDefinition part)
        {
            Current = part;
            Name = part.Name;
            PartName = part.PartDefinition.Name;
            TypeName = part.ContentTypeDefinition != null ? part.ContentTypeDefinition.Name : default(string);
            _settings = new JObject(part.Settings);
        }

        public ContentTypePartDefinitionBuilder WithSetting(string name, string value)
        {
            _settings[name] = value;
            return this;
        }

        public abstract ContentTypePartDefinition Build();
    }
}