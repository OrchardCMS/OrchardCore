using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class ContentTypeCreateResourceBuilder
    {
        private IDictionary<string, object> _values = new Dictionary<string, object>();

        private List<ContentPartBuilder> contentPartBuilders = new List<ContentPartBuilder>();

        public ContentTypeCreateResourceBuilder(string contentType)
        {
            _values.Add("ContentType".ToGraphQLStringFormat(), contentType);
        }

        public ContentPartBuilder WithContentPart(string contentPartName) {
            var builder = new ContentPartBuilder(contentPartName);
            contentPartBuilders.Add(builder);
            return builder;
        }

        public ContentTypeCreateResourceBuilder WithField(string key, object value)
        {
            _values.Add(key.ToGraphQLStringFormat(), value);

            return this;
        }

        internal string Build()
        {
            var sbo = new StringBuilder();

            foreach (var value in _values)
            {
                if (value.Value is string)
                {
                    sbo.AppendLine($"{value.Key}: \"{value.Value}\"");
                }
                else if (value.Value is bool)
                {
                    sbo.AppendLine($"{value.Key}: {((bool)value.Value).ToString().ToLowerInvariant()}");
                }
                else {
                    sbo.AppendLine($"{value.Key}: {value.Value}");
                }
            }

            sbo.AppendLine("contentParts: {");

            for (var i = 0; i < contentPartBuilders.Count; i++)
            {
                sbo.AppendLine(contentPartBuilders[i].Build() + ((i == (contentPartBuilders.Count - 1)) ? "" : ","));
            }
            
            sbo.AppendLine("}");

            return sbo.ToString();
        }
    }
}
