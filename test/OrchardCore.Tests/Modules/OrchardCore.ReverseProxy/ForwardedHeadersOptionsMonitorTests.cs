using OrchardCore.ReverseProxy.Drivers;
using OrchardCore.ReverseProxy.ViewModels;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.ReverseProxy;

public class ForwardedHeadersOptionsMonitorTests
{
    [Fact]
    public async Task UpdateReverseProxySettings_WithDriver_ForwardsHeadersWithoutReleasingTenant()
    {
        // Deny all permissions to HTTP requests so that the admin challenges them with an absolute login URL,
        // which reflects the request scheme.
        using var context = new SiteContext()
            .WithRecipe("SaaS")
            .WithPermissionsContext(new PermissionsContext { UsePermissionsContext = true });

        await context.InitializeAsync();
        await context.EnableFeaturesAsync("OrchardCore.ReverseProxy");

        Assert.StartsWith("http://", (await GetAdminWithForwardedProtoAsync(context)).Headers.Location.OriginalString);

        var shellTicks = await context.GetShellTicksAsync();

        await context.UpdateSiteSettingsWithDriverAsync<ReverseProxySettingsDisplayDriver>(ReverseProxySettingsDisplayDriver.GroupId, model =>
        {
            var viewModel = Assert.IsType<ReverseProxySettingsViewModel>(model);
            viewModel.EnableXForwardedProto = true;
        });

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        Assert.StartsWith("https://", (await GetAdminWithForwardedProtoAsync(context)).Headers.Location.OriginalString);
    }

    private static async Task<HttpResponseMessage> GetAdminWithForwardedProtoAsync(SiteContext context)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "Admin");
        request.Headers.Add("X-Forwarded-Proto", "https");

        var response = await context.Client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        return response;
    }
}
