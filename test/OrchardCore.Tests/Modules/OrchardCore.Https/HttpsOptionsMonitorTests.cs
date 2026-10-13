using OrchardCore.Https.Drivers;
using OrchardCore.Https.Settings;
using OrchardCore.Https.ViewModels;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Https;

public class HttpsOptionsMonitorTests
{
    [Fact]
    public async Task UpdateHttpsSettings_RequireHttps_RedirectsWithoutReleasingTenant()
    {
        using var context = await CreateContextAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await context.Client.GetAsync("not-found", TestContext.Current.CancellationToken)).StatusCode);

        var shellTicks = await context.GetShellTicksAsync();

        await UpdateSettingsAsync(context, model =>
        {
            model.RequireHttps = true;
            model.SslPort = 5001;
        });

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());

        var response = await context.Client.GetAsync("not-found", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.TemporaryRedirect, response.StatusCode);
        Assert.Equal($"https://localhost:5001/{context.TenantName}/not-found", response.Headers.Location.OriginalString);

        await UpdateSettingsAsync(context, model =>
        {
            model.RequireHttps = true;
            model.RequireHttpsPermanent = true;
            model.SslPort = 5002;
        });

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());

        response = await context.Client.GetAsync("not-found", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.PermanentRedirect, response.StatusCode);
        Assert.Equal($"https://localhost:5002/{context.TenantName}/not-found", response.Headers.Location.OriginalString);

        await UpdateSettingsAsync(context, model => model.RequireHttps = false);

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        Assert.Equal(HttpStatusCode.NotFound, (await context.Client.GetAsync("not-found", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task UpdateHttpsSettings_StrictTransportSecurityMode_AppliesHstsWithoutReleasingTenant()
    {
        using var context = await CreateContextAsync();

        Assert.False((await GetOverHttpsAsync(context)).Headers.Contains("Strict-Transport-Security"));

        var shellTicks = await context.GetShellTicksAsync();

        await UpdateSettingsAsync(context, model => model.StrictTransportSecurityMode = HttpStrictTransportSecurityMode.Enabled);

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        Assert.Equal("max-age=31536000; includeSubDomains", (await GetOverHttpsAsync(context)).Headers.GetValues("Strict-Transport-Security").Single());

        await UpdateSettingsAsync(context, model => model.StrictTransportSecurityMode = HttpStrictTransportSecurityMode.Disabled);

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        Assert.False((await GetOverHttpsAsync(context)).Headers.Contains("Strict-Transport-Security"));

        // HSTS is only applied from configuration in the 'Production' environment.
        await UpdateSettingsAsync(context, model => model.StrictTransportSecurityMode = HttpStrictTransportSecurityMode.FromConfiguration);

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        Assert.Equal(
            SiteContext.Site.Services.GetRequiredService<IHostEnvironment>().IsProduction(),
            (await GetOverHttpsAsync(context)).Headers.Contains("Strict-Transport-Security"));
    }

    private static async Task<SiteContext> CreateContextAsync()
    {
        var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();
        await context.EnableFeaturesAsync("OrchardCore.Https");

        return context;
    }

    private static Task UpdateSettingsAsync(SiteContext context, Action<HttpsSettingsViewModel> update)
        => context.UpdateSiteSettingsWithDriverAsync<HttpsSettingsDisplayDriver>(
            HttpsSettingsDisplayDriver.GroupId,
            model => update(Assert.IsType<HttpsSettingsViewModel>(model)),
            httpContext => httpContext.Request.Scheme = "https");

    private static Task<HttpResponseMessage> GetOverHttpsAsync(SiteContext context)
        => context.Client.GetAsync(
            new Uri($"https://www.example.com/{context.TenantName}/not-found"),
            TestContext.Current.CancellationToken);
}
