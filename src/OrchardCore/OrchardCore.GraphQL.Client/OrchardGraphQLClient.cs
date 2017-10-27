using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.GraphQL.Client
{
    public class OrchardGraphQLClient
    {
        private readonly HttpClient _client;

        public OrchardGraphQLClient(HttpClient client)
        {
            _client = client;
        }

        public TenantResource Tenants => new TenantResource(_client);

        public ContentResource Content => new ContentResource(_client);
    }

    public class ContentResource
    {
        private HttpClient _client;

        public ContentResource(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> CreateAsync(string contentType, Action<ContentTypeCreateResourceBuilder> builder) {
            var contentTypeBuilder = new ContentTypeCreateResourceBuilder(contentType);
            builder(contentTypeBuilder);

            var variables = contentTypeBuilder.Build();

            var json = @"{
  ""query"": ""mutation ($contentItem: ContentItemInput!){ createContentItem(contentItem: $contentItem) { contentItemId } }"",
  ""variables"": " + JsonConvert.SerializeObject(variables) + @"}";

            var response = await _client.PostJsonAsync("graphql", json);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            return result["data"]["createContentItem"]["contentItemId"].ToString();
        }

        public async Task<JObject> QueryAsync(string contentType, Action<ContentTypeQueryResourceBuilder> builder)
        {
            var contentTypeBuilder = new ContentTypeQueryResourceBuilder(contentType);
            builder(contentTypeBuilder);

            var variables = "query { " + contentTypeBuilder.Build() + " }";

            var response = await _client
                .GetAsync("graphql?query=" + HttpUtility.UrlEncode(variables));

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }
    }

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

            var variables =
@"{ 
    ""contentItem"": {
        ""contentType"": """+ _contentType + @""", 
        ""contentParts"": " + JsonConvert.SerializeObject("{" + sb.ToString() + "}") + @"
    }
}";

            return variables;
        }
    }

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
            _keys.Add(name.ToGraphQLStringFormat());

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

        internal string Build()
        {
            var sb = new StringBuilder(_contentType.ToGraphQLStringFormat());

            if (_queryFields.Count > 0 || _nestedQueryFields.Count > 0)
            {
                sb.Append("(");

                for (var i = 0; i < _nestedQueryFields.Count; i++)
                {
                    var item = _nestedQueryFields.ElementAt(i);
                    sb.AppendFormat("{0}: {{ {1} }}", item.Key.ToGraphQLStringFormat(), item.Value);

                    if (i < (_nestedQueryFields.Count - 1))
                    {
                        sb.Append(" ");
                    }
                }

                for (var i = 0; i < _queryFields.Count; i++)
                {
                    var item = _queryFields.ElementAt(i);
                    sb.AppendFormat("{0}: {{ {1} }}", item.Key.ToGraphQLStringFormat(), item.Value);

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

            return sb.ToString();
        }
    }

    public class ContentPartBuilder
    {
        private string _contentPartName;

        private Dictionary<string, string> _keysWithValues = new Dictionary<string, string>();
        private List<string> _keys = new List<string>();

        public ContentPartBuilder(string contentPartName)
        {
            _contentPartName = contentPartName;
        }

        public ContentPartBuilder AddField(string name, string value)
        {
            _keysWithValues.Add(name, value);

            return this;
        }

        public ContentPartBuilder AddField(string name)
        {
            _keys.Add(name);

            return this;
        }

        internal string Build() {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}: {{ ", _contentPartName.ToGraphQLStringFormat());


            for (var i = 0; i < _keysWithValues.Count; i++)
            {
                var item = _keysWithValues.ElementAt(i);
                sb.AppendFormat("{0}: \"{1}\"", item.Key.ToGraphQLStringFormat(), item.Value);

                if (i < (_keysWithValues.Count - 1))
                {
                    sb.Append(" ");
                }
            }

            foreach (var item in _keys)
            {
                sb.Append(item.ToGraphQLStringFormat() + " ");
            }

            sb.Append(" }");

            return sb.ToString();
        }
    }
}
