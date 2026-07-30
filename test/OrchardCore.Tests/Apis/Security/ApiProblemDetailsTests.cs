using System.Net;
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

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("\"status\":401", body);
        Assert.Contains("\"title\"", body);
    }
}
