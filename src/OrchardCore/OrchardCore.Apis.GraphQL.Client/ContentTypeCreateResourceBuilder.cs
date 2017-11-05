using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Apis.GraphQL.Client
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
                    "ContentItem",
                    new JObject(
                        new JProperty("ContentType", _contentType),
                        new JProperty("ContentParts", "{" + sb.ToString() + "}")
                    )
                )
            );

            return variables.ToString();
        }
    }
}
