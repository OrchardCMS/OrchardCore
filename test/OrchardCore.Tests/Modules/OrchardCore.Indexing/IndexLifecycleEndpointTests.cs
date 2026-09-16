using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Endpoints.Management;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexLifecycleEndpointTests
{
    [Fact]
    public async Task Denied_DoesNotReadOrQueueOperations()
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        var profiles = new Mock<IIndexProfileManager>(MockBehavior.Strict);
        var authorization = Authorize(false);
        Assert.Equal(403, Status(await IndexLifecycleEndpoints.QueueAsync(context, authorization, profiles.Object, null, null, "index", IndexLifecycleAction.Reset)));
        Assert.Equal(403, Status(await IndexLifecycleEndpoints.GetAsync(context, authorization, null, "operation")));
        profiles.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Queue_UnverifiedProvider_RejectsBeforeScheduling()
    {
        var profiles = new Mock<IIndexProfileManager>();
        profiles.Setup(value => value.FindByIdAsync("index")).ReturnsAsync(new IndexProfile
        {
            Id = "index", ProviderName = "Unverified", Type = IndexingConstants.ContentsIndexSource,
        });
        var result = await IndexLifecycleEndpoints.QueueAsync(new DefaultHttpContext(), Authorize(true), profiles.Object,
            null, Options.Create(new IndexLifecycleOptions()), "index", IndexLifecycleAction.Rebuild);
        Assert.Equal(501, Status(result));
    }

    private static int? Status(IResult result) => Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode;

    private static IAuthorizationService Authorize(bool allowed)
    {
        var authorization = new Mock<IAuthorizationService>();
        authorization.Setup(value => value.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(allowed ? AuthorizationResult.Success() : AuthorizationResult.Failed());
        return authorization.Object;
    }
}
