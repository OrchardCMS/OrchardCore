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

        public ContentPartBuilder WithContentPart(string contentPartName)
        {
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

            sbo.Append(ContentType.ToGraphQLStringFormat()).AppendLine(": {");

            foreach (var value in _values)
            {
                var key = value.Key;

                if (value.Value is string)
                {
                    sbo.Append(key).Append(": \"").Append(value.Value).Append('"').AppendLine();
                }
                else if (value.Value is bool)
                {
                    sbo.Append(key).Append(": ").Append(((bool)value.Value).ToString().ToLowerInvariant()).AppendLine();
                }
                else
                {
                    sbo.Append(key).Append(": ").Append(value.Value).AppendLine();
                }
            }

            for (var i = 0; i < contentPartBuilders.Count; i++)
            {
                sbo.Append(contentPartBuilders[i].Build()).AppendLine((i == (contentPartBuilders.Count - 1)) ? "" : ",");
            }

            sbo.Append('}').AppendLine();

            return sbo.ToString();
        }
    }
}
