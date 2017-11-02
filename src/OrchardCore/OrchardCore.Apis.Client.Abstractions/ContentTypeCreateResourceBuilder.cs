using System.Collections.Generic;

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

        public virtual string Build() { return null; }
    }
}
