using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.Apis.GraphQL.Client;

public class ContentResource
{
    private readonly HttpClient _client;

    public ContentResource(HttpClient client)
    {
        _client = client;
    }

    public async Task<JsonObject> Query(string contentType, Action<ContentTypeQueryResourceBuilder> builder)
    {
        var contentTypeBuilder = new ContentTypeQueryResourceBuilder(contentType);
        builder(contentTypeBuilder);

        var requestJson = new JsonObject
        {
            ["query"] = @"query { " + contentTypeBuilder.Build() + " }",
        };

        var response = await _client
            .PostJsonAsync("api/graphql", requestJson.ToJsonString(JOptions.Default));

        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
        {
            throw new Exception(response.StatusCode.ToString() + " " + await response.Content.ReadAsStringAsync());
        }

        return JObject.Parse(await response.Content.ReadAsStringAsync());
    }

    public async Task<JsonObject> Query(string body)
    {
        var requestJson = new JsonObject
        {
            ["query"] = @"query { " + body + " }",
        };

        var response = await _client.PostJsonAsync("api/graphql", requestJson.ToJsonString(JOptions.Default));

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(response.StatusCode.ToString() + " " + await response.Content.ReadAsStringAsync());
        }

        return JObject.Parse(await response.Content.ReadAsStringAsync());
    }

    public async Task<JsonObject> NamedQueryExecute(string name)
    {
        var requestJson = new JsonObject
        {
            ["namedquery"] = name,
        };

        var response = await _client.PostJsonAsync("api/graphql", requestJson.ToJsonString(JOptions.Default));

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(response.StatusCode.ToString() + " " + await response.Content.ReadAsStringAsync());
        }

        return JObject.Parse(await response.Content.ReadAsStringAsync());
    }
}
