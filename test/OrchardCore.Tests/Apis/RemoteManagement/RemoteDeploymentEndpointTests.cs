using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Remote.Endpoints;
using OrchardCore.Deployment.Remote.Models;
using OrchardCore.Deployment.Remote.Services;
using OrchardCore.Deployment.Services;
using OrchardCore.Documents;
using OrchardCore.Recipes.Models;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class RemoteDeploymentEndpointTests
{
    [Theory]
    [InlineData("key", "key", true)]
    [InlineData("key", "other", false)]
    [InlineData("key", "key1", false)]
    [InlineData("key", null, false)]
    [InlineData(null, "key", false)]
    [InlineData(null, null, true)]
    public void SameApiKey_ComparesTheWholeValue_ReturnsWhetherTheyMatch(string stored, string candidate, bool expected)
    {
        Assert.Equal(expected, RemoteDeploymentEndpoints.SameApiKey(stored, candidate));
    }

    [Fact]
    public void Map_AllRoutes_HaveOperationIdsWithTheApiPrefix()
    {
        var app = WebApplication.CreateBuilder().Build();

        RemoteDeploymentEndpoints.Map(app);
        RemoteDeploymentTargetEndpoints.Map(app);

        var names = ((IEndpointRouteBuilder)app).DataSources.SelectMany(source => source.Endpoints)
            .Select(endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName)
            .ToArray();

        Assert.Equal(12, names.Length);
        Assert.Equal(names.Length, names.Distinct().Count());
        Assert.All(names, name => Assert.StartsWith("Api", name));
        Assert.Contains("ApiListRemoteDeploymentClients", names);
        Assert.Contains("ApiGetRemoteDeploymentInstance", names);
        Assert.Contains("ApiSendToRemoteDeploymentTarget", names);
    }

    [Fact]
    public async Task Send_TargetFailsInAnyWay_ReturnsTheSameProblem()
    {
        var responses = new List<string>();

        foreach (var handler in new Func<Task<HttpResponseMessage>>[]
        {
            () => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)),
            () => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)),
            () => Task.FromException<HttpResponseMessage>(new HttpRequestException("No such host is known.")),
            () => Task.FromException<HttpResponseMessage>(new TaskCanceledException("Timed out.")),
        })
        {
            var result = await SendAsync(handler);
            var problem = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
            Assert.Equal(502, problem.StatusCode);
            responses.Add(JsonSerializer.Serialize(((IValueHttpResult)result).Value));
        }

        Assert.Single(responses.Distinct());
    }

    [Fact]
    public async Task Send_TargetAccepts_ReportsSuccess()
    {
        var result = await SendAsync(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        Assert.Equal(200, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    private static Task<IResult> SendAsync(Func<Task<HttpResponseMessage>> respond)
    {
        var documents = new Mock<IDocumentManager<RemoteInstanceList>>();
        documents.Setup(value => value.GetOrCreateImmutableAsync(It.IsAny<Func<Task<RemoteInstanceList>>>())).ReturnsAsync(new RemoteInstanceList
        {
            RemoteInstances = [new RemoteInstance { Id = "target", Name = "Target", Url = "https://target.example.com/import", ClientName = "client", ApiKey = "key" }],
        });

        var session = new Mock<ISession>();
        session.Setup(value => value.GetAsync<DeploymentPlan>(It.IsAny<long[]>(), It.IsAny<string>()))
            .ReturnsAsync(new[] { new DeploymentPlan { Id = 1, Name = "plan" } });

        var archives = new Mock<IDeploymentArchiveService>();
        archives.Setup(value => value.CreateAsync(It.IsAny<DeploymentPlan>(), It.IsAny<RecipeDescriptor>()))
            .ReturnsAsync(() => new MemoryStream());

        var clients = new Mock<IHttpClientFactory>();
        clients.Setup(value => value.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(new Handler(respond)));

        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var context = new DefaultHttpContext();
        context.Request.IsHttps = true;

        return RemoteDeploymentTargetEndpoints.SendAsync("target", context, new RemoteInstanceService(documents.Object), authorization.Object,
            session.Object, archives.Object, new RemoteDeploymentSender(clients.Object), new RemoteDeploymentSendRequest { PlanId = 1 });
    }

    private sealed class Handler : HttpMessageHandler
    {
        private readonly Func<Task<HttpResponseMessage>> _respond;

        public Handler(Func<Task<HttpResponseMessage>> respond)
        {
            _respond = respond;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => _respond();
    }
}
