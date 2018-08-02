using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Apis.JsonApi.Client.Builders
{
    public class ContentTypeCreateResourceBuilder
    {
        private List<ContentPartCreateResourceBuilder> _contentPartBuilders = new List<ContentPartCreateResourceBuilder>();

        public ContentTypeCreateResourceBuilder(string contentType)
        {
            ContentType = contentType;
        }

        public readonly string ContentType;

        public ContentTypeCreateResourceBuilder WithContentPart(string contentPartName, Action<ContentPartCreateResourceBuilder> builder) {
            var contentPartBuilder = new ContentPartCreateResourceBuilder(contentPartName);
            builder(contentPartBuilder);
            _contentPartBuilders.Add(contentPartBuilder);
            return this;
        }

        internal string Build()
        {
            var value = new JObject(
                new JProperty("type", ContentType),
                new JProperty("attributes", new JArray(new JObject(_contentPartBuilders.Select(cpb => cpb.Build()))))
                );

            return value.ToString();
        }
    }
}
