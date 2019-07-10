using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class ContentTypeQueryResourceBuilder
    {
        private string _contentType;

        private List<string> _keys = new List<string>();
        private IDictionary<string, object> _queries = new Dictionary<string, object>();
        private List<ContentTypeQueryResourceBuilder> _nested = new List<ContentTypeQueryResourceBuilder>();

        public ContentTypeQueryResourceBuilder(string contentType)
        {
            _contentType = contentType.ToGraphQLStringFormat();
        }

        public ContentTypeQueryResourceBuilder WithField(string name)
        {
            _keys.Add(name.ToGraphQLStringFormat());

            return this;
        }

        public ContentTypeQueryResourceBuilder WithNestedField(string fieldName)
        {
            var builder = new ContentTypeQueryResourceBuilder(fieldName);
            _nested.Add(builder);
            return builder;
        }

        public ContentTypeQueryResourceBuilder WithQueryArgument(string argument, string value)
        {
            if (_queries.ContainsKey(argument))
            {
                throw new Exception($"Argument already exists: {argument}");
            }
            else
            {
                _queries.Add(argument, value);
            }

            return this;
        }

        public ContentTypeQueryResourceBuilder WithQueryStringArgument(string argument, string value)
        {
            return WithQueryArgument(argument, $"\"{value}\"");
        }

        public ContentTypeQueryResourceBuilder WithQueryArgument(string argument, string fieldName, string fieldValue)
        {
            // Top-level argument exists
            if (_queries.TryGetValue(argument, out var queryValues) && queryValues is string)
            {
                throw new Exception($"Argument already exists: {argument}");
            }
            // Field-level argument exists
            else if (queryValues is IDictionary<string, string>)
            {
                ((IDictionary<string, string>) queryValues)
                    .Add(fieldName.ToGraphQLStringFormat(), fieldValue);
            }
            else
            {
                _queries.Add(argument, new Dictionary<string, string>()
                {
                    { fieldName.ToGraphQLStringFormat(),  fieldValue }
                });
            }

            return this;
        }

        public ContentTypeQueryResourceBuilder WithQueryStringArgument(string argument, string fieldName, string fieldValue)
        {
            return WithQueryArgument(argument, fieldName, $"\"{fieldValue}\"");
        }

        public ContentTypeQueryResourceBuilder WithNestedQueryArgument(string argument, string fieldName, string fieldValue)
        {
            // Top-level argument exists
            if (_queries.TryGetValue(argument, out var queryValues) && queryValues is string)
            {
                throw new Exception($"Argument already exists: {argument}");
            }
            // Field-level argument exists
            else if (queryValues is IDictionary<string, string>)
            {
                ((IDictionary<string, string>)queryValues)
                    .Add(fieldName.ToGraphQLStringFormat(), $"{{ {fieldValue} }}");
            }
            else
            {
                _queries.Add(argument, new Dictionary<string, string>()
                {
                    { fieldName.ToGraphQLStringFormat(),  $"{{ {fieldValue} }}" }
                });
            }

            return this;
        }

        internal string Build()
        {
            var sb = new StringBuilder(_contentType);

            if (_queries.Count > 0)
            {
                sb.Append("(");

                for (var i = 0; i < _queries.Count; i++)
                {
                    var query = _queries.ElementAt(i);

                    // Top-level argument
                    if (query.Value is string)
                    {
                        sb.Append($"{query.Key}: {query.Value}");
                    }
                    // Field-level argument
                    else
                    {
                        sb.Append($"{query.Key}:{{");

                        var fieldValuePair = (IDictionary<string, string>)query.Value;
                        for (var c = 0; c < fieldValuePair.Count; c++)
                        {
                            var item = fieldValuePair.ElementAt(c);

                            sb.Append($"{item.Key}: {item.Value}");

                            if (c < (fieldValuePair.Count - 1))
                            {
                                sb.Append(", ");
                            }
                            else
                            {
                                sb.Append("}");
                            }
                        }

                        if (i < (_queries.Count - 1))
                        {
                            sb.Append(", ");
                        }
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
