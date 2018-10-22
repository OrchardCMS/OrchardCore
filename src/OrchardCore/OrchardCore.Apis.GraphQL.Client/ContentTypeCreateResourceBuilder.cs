using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class ContentTypeCreateResourceBuilder
    {
        private IDictionary<string, object> _values = new Dictionary<string, object>();

        private List<ContentPartBuilder> contentPartBuilders = new List<ContentPartBuilder>();

        private string ContentType { get; set; }

        public ContentTypeCreateResourceBuilder(string contentType)
        {
            ContentType = contentType;
        }

        public ContentPartBuilder WithContentPart(string contentPartName) {
            var builder = new ContentPartBuilder(contentPartName.ToGraphQLStringFormat());
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
            var sbo = new StringBuilder();

            sbo.AppendLine(ContentType.ToGraphQLStringFormat() + ": {");

            foreach (var value in _values)
            {
                var key = value.Key;

                if (value.Value is string)
                {
                    sbo.AppendLine($"{key}: \"{value.Value}\"");
                }
                else if (value.Value is bool)
                {
                    sbo.AppendLine($"{key}: {((bool)value.Value).ToString().ToLowerInvariant()}");
                }
                else {
                    sbo.AppendLine($"{key}: {value.Value}");
                }
            }

            for (var i = 0; i < contentPartBuilders.Count; i++)
            {
                sbo.AppendLine(contentPartBuilders[i].Build() + ((i == (contentPartBuilders.Count - 1)) ? "" : ","));
            }
            
            sbo.AppendLine("}");

            return sbo.ToString();
        }
    }
}
