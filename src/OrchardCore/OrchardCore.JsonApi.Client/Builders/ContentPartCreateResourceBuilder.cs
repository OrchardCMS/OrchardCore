using System.Collections.Generic;

namespace OrchardCore.JsonApi.Client.Builders
{
    public class ContentPartCreateResourceBuilder
    {
        private Dictionary<string, string> _properties = new Dictionary<string, string>();

        public ContentPartCreateResourceBuilder(string contentPartName)
        {
            ContentPartName = contentPartName;
        }

        public readonly string ContentPartName;

        public ContentPartCreateResourceBuilder WithProperty(string name, string value)
        {
            _properties.Add(name, value);

            return this;
        }
        
        internal string Build() {
            return null;
        }
    }
}
