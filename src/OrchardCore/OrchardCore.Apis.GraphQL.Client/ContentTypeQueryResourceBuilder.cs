using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class ContentTypeQueryResourceBuilder
    {
        private readonly string _contentType;
        private readonly List<string> _keys = new();
        private readonly Dictionary<string, object> _queries = new();
        private readonly List<ContentTypeQueryResourceBuilder> _nested = new();

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
                ((IDictionary<string, string>)queryValues)
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

        public string Build()
        {
            var sb = new StringBuilder(_contentType);

            if (_queries.Count > 0)
            {
                sb.Append('(');

                for (var i = 0; i < _queries.Count; i++)
                {
                    var query = _queries.ElementAt(i);

                    if (i > 0)
                    {
                        sb.Append(' ');
                    }

                    // Top-level argument
                    if (query.Value is string)
                    {
                        sb.Append(query.Key).Append(": ").Append(query.Value);
                    }
                    // Field-level argument
                    else
                    {
                        sb.Append(query.Key).Append(":{");

                        var fieldValuePair = (IDictionary<string, string>)query.Value;
                        for (var c = 0; c < fieldValuePair.Count; c++)
                        {
                            var item = fieldValuePair.ElementAt(c);

                            sb.Append(item.Key).Append(": ").Append(item.Value);

                            if (c < (fieldValuePair.Count - 1))
                            {
                                sb.Append(", ");
                            }
                            else
                            {
                                sb.Append('}');
                            }
                        }

                        if (i < (_queries.Count - 1))
                        {
                            sb.Append(", ");
                        }
                    }
                }

                sb.Append(')');
            }

            var hasFields = _keys.Count > 0 || _nested.Count > 0;

            sb.Append(hasFields ? " { " : " {");

            sb.AppendJoin(' ', _keys);

            if (_nested.Count > 0)
            {
                if (_keys.Count > 0)
                {
                    sb.Append(' ');
                }

                var first = true;

                foreach (var item in _nested)
                {
                    if (!first)
                    {
                        sb.Append(' ');
                    }

                    sb.Append(item.Build());
                    first = false;
                }
            }

            sb.Append(hasFields ? " }" : "}");
            return sb.ToString().TrimStart();
        }
    }
}
