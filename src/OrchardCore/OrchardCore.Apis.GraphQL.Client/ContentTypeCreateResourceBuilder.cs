using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class ContentTypeCreateResourceBuilder
    {
        private readonly Dictionary<string, object> _values = new();
        private readonly List<ContentPartBuilder> _contentPartBuilders = new();

        private string ContentType { get; set; }

        public ContentTypeCreateResourceBuilder(string contentType)
        {
            ContentType = contentType;
        }

        public ContentPartBuilder WithContentPart(string contentPartName)
        {
            var builder = new ContentPartBuilder(contentPartName.ToGraphQLStringFormat());
            _contentPartBuilders.Add(builder);
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

            for (var i = 0; i < _contentPartBuilders.Count; i++)
            {
                sbo.Append(_contentPartBuilders[i].Build()).AppendLine((i == (_contentPartBuilders.Count - 1)) ? String.Empty : ",");
            }

            sbo.Append('}').AppendLine();

            return sbo.ToString();
        }
    }
}
