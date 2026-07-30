using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.Security;

public class ApiProblemDetailsTests
{
    /// <summary>
    /// A challenge issued through the "Api" authentication scheme must produce a response an API
    /// client can consume: the RFC 9110 §15.5.2 WWW-Authenticate header and an RFC 9457 Problem
    /// Details body, instead of an empty body or an HTML error page.
    /// </summary>
    [Fact]
    public async Task ApiChallengeCarriesWwwAuthenticateHeaderAndProblemDetailsBody()
    {
        using var context = new SiteContext()
            .WithPermissionsContext(new PermissionsContext { UsePermissionsContext = true });

        await context.InitializeAsync();

        var response = await context.Client.GetAsync("api/graphql", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("Bearer", response.Headers.WwwAuthenticate.ToString());
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonObject>(TestContext.Current.CancellationToken);

        Assert.Equal(401, (int)problemDetails["status"]);
        Assert.Equal("Authentication required", (string)problemDetails["title"]);
        Assert.Contains("WWW-Authenticate", (string)problemDetails["detail"]);
        Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.2", (string)problemDetails["type"]);
    }
}
