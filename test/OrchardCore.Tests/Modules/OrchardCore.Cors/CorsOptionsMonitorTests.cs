using OrchardCore.Cors.Services;
using OrchardCore.Cors.Settings;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Cors;

public class CorsOptionsMonitorTests
{
    private const string Origin = "https://www.example.com";

    [Fact]
    public async Task UpdateCorsSettings_WithCorsService_AppliesPoliciesWithoutReleasingTenant()
    {
        using var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();
        await context.EnableFeaturesAsync("OrchardCore.Cors");

        Assert.False((await SendPreflightRequestAsync(context)).Headers.Contains("Access-Control-Allow-Origin"));

        var shellTicks = await context.GetShellTicksAsync();

        await context.UsingTenantScopeAsync(scope =>
            scope.ServiceProvider.GetRequiredService<CorsService>().UpdateSettingsAsync(new CorsSettings
            {
                Policies =
                [
                    new CorsPolicySetting
                    {
                        Name = "Test",
                        AllowedOrigins = [Origin],
                        AllowAnyHeader = true,
                        AllowAnyMethod = true,
                        IsDefaultPolicy = true,
                    },
                ],
            }));

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        Assert.Equal(Origin, (await SendPreflightRequestAsync(context)).Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    private static Task<HttpResponseMessage> SendPreflightRequestAsync(SiteContext context)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "api/content");
        request.Headers.Add("Origin", Origin);
        request.Headers.Add("Access-Control-Request-Method", HttpMethods.Get);

        return context.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
