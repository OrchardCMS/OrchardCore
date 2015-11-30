using System.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement.MetaData.Builders
{
    public abstract class ContentPartFieldDefinitionBuilder
    {
        protected readonly JObject _settings;

        public ContentPartFieldDefinition Current { get; private set; }

        protected ContentPartFieldDefinitionBuilder(ContentPartFieldDefinition field)
        {
            Current = field;

            _settings = new JObject(field.Settings);
        }

        public ContentPartFieldDefinitionBuilder WithSetting(string name, string value)
        {
            _settings[name] = value;
            return this;
        }

        public ContentPartFieldDefinitionBuilder WithDisplayName(string displayName)
        {
            Current.DisplayName = displayName;
            return this;
        }

        public abstract string Name { get; }
        public abstract string FieldType { get; }
        public abstract string PartName { get; }

        public abstract ContentPartFieldDefinitionBuilder OfType(ContentFieldDefinition fieldDefinition);
        public abstract ContentPartFieldDefinitionBuilder OfType(string fieldType);

        public abstract ContentPartFieldDefinition Build();
    }
}