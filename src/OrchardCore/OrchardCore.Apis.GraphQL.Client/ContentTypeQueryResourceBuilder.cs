using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OrchardCore.Apis.GraphQL.Client
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
            _contentType = contentType.ToGraphQLStringFormat();
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
            _queryFields.Add(fieldName.ToGraphQLStringFormat(), fieldValue);
            return this;
        }

        public ContentTypeQueryResourceBuilder WithNestedQueryField(string fieldName, string fieldValue)
        {
            _nestedQueryFields.Add(fieldName.ToGraphQLStringFormat(), fieldValue);
            return this;
        }

        internal string Build()
        {
            var sb = new StringBuilder(_contentType);

            if (_queryFields.Count > 0 || _nestedQueryFields.Count > 0)
            {
                sb.Append("(");

                for (var i = 0; i < _nestedQueryFields.Count; i++)
                {
                    var item = _nestedQueryFields.ElementAt(i);
                    sb.AppendFormat("{0}: {{ {1} }}", item.Key, item.Value);

                    if (i < (_nestedQueryFields.Count - 1))
                    {
                        sb.Append(" ");
                    }
                }

                for (var i = 0; i < _queryFields.Count; i++)
                {
                    var item = _queryFields.ElementAt(i);
                    sb.AppendFormat("{0}: \"{1}\"", item.Key, item.Value);

                    if (i < (_queryFields.Count - 1))
                    {
                        sb.Append(" ");
                    }
                }

                sb.Append(")");
            }

            sb.Append(" { ");

            sb.Append(" " + string.Join(" ", _keys) + " ");

            foreach (var item in _nested)
            {
                sb.Append(item.Build());
            }

            sb.Append(" }");

            return sb.ToString().Trim();
        }
    }
}
