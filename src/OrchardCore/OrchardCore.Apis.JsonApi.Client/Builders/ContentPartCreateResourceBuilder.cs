using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Apis.JsonApi.Client.Builders
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
        
        internal JProperty Build() {
            return
               new JProperty(ContentPartName, new JArray(new JObject(_properties.Select(p => new JProperty(p.Key, p.Value)))));
        }
    }
}
