using System.Collections.Generic;

namespace OrchardCore.GraphQL.Client
{
    public class ContentTypeQueryResourceBuilder
    {
        private string _contentType;

        private List<string> _keys = new List<string>();
        private IDictionary<string, string> _queryFields = new Dictionary<string, string>();
        private IDictionary<string, string> _nestedQueryFields = new Dictionary<string, string>();
        private List<ContentTypeQueryResourceBuilder> _nested = new List<ContentTypeQueryResourceBuilder>();

        public ContentTypeQueryResourceBuilder(string contentType)
        {
            _contentType = contentType;
        }

        public ContentTypeQueryResourceBuilder AddField(string name)
        {
            _keys.Add(name);

            return this;
        }

        public ContentTypeQueryResourceBuilder WithNestedField(string fieldName)
        {
            var builder = new ContentTypeQueryResourceBuilder(fieldName);
            _nested.Add(builder);
            return builder;
        }

        public ContentTypeQueryResourceBuilder WithQueryField(string fieldName, string fieldValue)
        {
            _queryFields.Add(fieldName, fieldValue);
            return this;
        }

        public ContentTypeQueryResourceBuilder WithNestedQueryField(string fieldName, string fieldValue)
        {
            _nestedQueryFields.Add(fieldName, fieldValue);
            return this;
        }

        public virtual string Build() { return null; }
    }
}
