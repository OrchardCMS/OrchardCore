using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.GraphQL.Client
{
    public class ContentTypeCreateResourceBuilder
    {
        private string _contentType;

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

        internal string Build()
        {
            var sb = new StringBuilder();

            foreach (var cpb in contentPartBuilders)
            {
                sb.AppendLine(cpb.Build() + ",");
            }

            var variables = new JObject(
                new JProperty(
                    "contentItem",
                    new JObject(
                        new JProperty("contentType", _contentType),
                        new JProperty("contentParts", "{" + sb.ToString() + "}")
                    )
                )
            );

            return variables.ToString();
        }
    }
}
