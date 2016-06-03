using System.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement.MetaData.Builders
{
    public abstract class ContentTypePartDefinitionBuilder
    {
        protected readonly JObject _settings;

        protected ContentTypePartDefinitionBuilder(ContentTypePartDefinition part)
        {
            Name = part.PartDefinition.Name;
            TypeName = part.ContentTypeDefinition != null ? part.ContentTypeDefinition.Name : default(string);
            _settings = new JObject(part.Settings);
        }

        public string Name { get; private set; }
        public string TypeName { get; private set; }

        public ContentTypePartDefinitionBuilder WithSetting(string name, string value)
        {
            _settings[name] = value;
            return this;
        }

        public abstract ContentTypePartDefinition Build();
    }
}