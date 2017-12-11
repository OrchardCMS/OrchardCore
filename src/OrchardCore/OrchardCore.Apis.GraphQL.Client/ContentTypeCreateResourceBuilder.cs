using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class ContentTypeCreateResourceBuilder
    {
        private string _contentType;
        private IDictionary<string, object> _values = new Dictionary<string, object>();

        private List<ContentPartBuilder> contentPartBuilders = new List<ContentPartBuilder>();

        public ContentTypeCreateResourceBuilder(string contentType)
        {
            _contentType = contentType;
        }

        public ContentPartBuilder WithContentPart(string contentPartName) {
            var builder = new ContentPartBuilder(contentPartName);
            contentPartBuilders.Add(builder);
            return builder;
        }

        public ContentTypeCreateResourceBuilder WithField(string key, object value)
        {
            _values.Add(key, value);

            return this;
        }

        internal string Build()
        {
            var sb = new StringBuilder();

            foreach (var cpb in contentPartBuilders)
            {
                sb.AppendLine(cpb.Build() + ",");
            }

            StringBuilder sbo = new StringBuilder();

            sbo.AppendFormat(" ContentType: \"{0}\"", _contentType);

            foreach (var value in _values)
            {
                if (value.Value is string)
                {
                    sbo.AppendFormat(" {0}: \"{1}\"", value.Key, value.Value);
                }
                else if (value.Value is bool)
                {
                    sbo.AppendFormat(" {0}: {1}", value.Key, ((bool)value.Value).ToString().ToLowerInvariant());
                }
                else {
                    sbo.AppendFormat(" {0}: {1}", value.Key, value.Value);
                }
            }

            sbo.AppendFormat(" ContentParts: {0}", JsonConvert.SerializeObject("{ " + sb.ToString() + " }"));

            return sbo.ToString();
        }
    }
}
