using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Media;
using OrchardCore.Media.Endpoints.Api;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class GetUploadOptionsEndpointTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(30000000)]
    public async Task HandleAsync_Authorized_ReturnsTenantLimit(long maxFileSize)
    {
        var (result, context) = await InvokeAsync(true, maxFileSize);

        var response = Assert.IsType<Ok<MediaUploadOptionsDto>>(result);
        Assert.Equal(maxFileSize, response.Value.MaxFileSize);
        Assert.Equal("no-store", context.Response.Headers.CacheControl);
    }

    [Fact]
    public async Task HandleAsync_Unauthorized_ReturnsForbidden()
    {
        var (result, _) = await InvokeAsync(false, 10);

        var response = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, response.StatusCode);
    }

    [Fact]
    public void HandleAsync_Endpoint_UsesMediaApiAuthenticationPolicy()
    {
        var method = typeof(GetUploadOptionsEndpoint).GetMethod("HandleAsync", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.Equal(MediaApiConstants.AuthorizationPolicyName, method.GetCustomAttribute<AuthorizeAttribute>().Policy);
    }

    private static async Task<(IResult Result, HttpContext Context)> InvokeAsync(bool authorized, long maxFileSize)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService.Setup(service => service.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(),
            It.IsAny<object>(),
            It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(authorized ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        var localizer = new Mock<IStringLocalizer<ProblemDetailsApiLocalization>>();
        localizer.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        using var services = new ServiceCollection().AddSingleton(localizer.Object).BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        var method = typeof(GetUploadOptionsEndpoint).GetMethod("HandleAsync", BindingFlags.NonPublic | BindingFlags.Static);
        var task = (Task<IResult>)method.Invoke(null, [
            context,
            authorizationService.Object,
            Options.Create(new MediaOptions { MaxFileSize = maxFileSize }),
        ]);

        return (await task, context);
    }
}
