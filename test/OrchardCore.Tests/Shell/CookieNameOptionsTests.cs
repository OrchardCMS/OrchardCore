using OrchardCore.Environment.Shell;
using OrchardCore.Extensions;

namespace OrchardCore.Tests.Shell;

public class CookieNameOptionsTests
{
    [Fact]
    public void PostConfigure_NoConfiguration_KeepsDefaultCookieNames()
    {
        var options = CreateOptions([]);

        Assert.Equal("__orchantiforgery_v1", options.Antiforgery);
        Assert.Equal("orchauth_My+Tenant", options.Authentication);
        Assert.Equal("orchemail_My+Tenant", options.EmailConfirmation);
        Assert.Equal("orchpasskey_My+Tenant", options.Passkey);
        Assert.Equal("Identity.External", options.ExternalAuthentication);
        Assert.Equal("Identity.TwoFactorUserId", options.TwoFactorUserId);
        Assert.Equal("Identity.TwoFactorRememberMe", options.TwoFactorRememberMe);
        Assert.Equal("orch_notify", options.Notify);
        Assert.Equal(".AspNetCore.Mvc.CookieTempDataProvider", options.TempData);
        Assert.Equal(".AspNetCore.Culture", options.Culture);
        Assert.Equal(".AspNet.Consent", options.Consent);
        Assert.Equal("admin_culture_v1", options.AdminCulture);
        Assert.Equal("orchardcore_admin_list_layout", options.AdminListLayout);
        Assert.Equal("My Tenant-admintheme", options.AdminTheme);
        Assert.Equal("My Tenant-TheAdmin-scheme", options.AdminScheme);
        Assert.Equal("My Tenant-adminPreferences", options.AdminPreferences);
    }

    [Fact]
    public void PostConfigure_ConfiguredNames_UsesConfiguredNames()
    {
        var options = CreateOptions(new Dictionary<string, string>
        {
            ["Cookies:Authentication"] = "custom_auth",
            ["Cookies:Antiforgery"] = "custom_antiforgery",
            ["Cookies:Notify"] = "custom_notify",
            ["Cookies:AdminScheme"] = "custom_scheme",
            ["Cookies:Culture"] = " ",
        });

        Assert.Equal("custom_auth", options.Authentication);
        Assert.Equal("custom_antiforgery", options.Antiforgery);
        Assert.Equal("custom_notify", options.Notify);
        Assert.Equal("custom_scheme", options.AdminScheme);
        Assert.Equal(".AspNetCore.Culture", options.Culture);
        Assert.Equal("orchpasskey_My+Tenant", options.Passkey);
    }

    private static CookieNameOptions CreateOptions(Dictionary<string, string> values)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var shellSettings = new ShellSettings { Name = "My Tenant", VersionId = "v1" };

        var services = new ServiceCollection()
            .Configure<CookieNameOptions>(configuration.GetSection("Cookies"))
            .AddSingleton(shellSettings)
            .AddSingleton<IPostConfigureOptions<CookieNameOptions>, CookieNameOptionsSetup>();

        return services.BuildServiceProvider().GetRequiredService<IOptions<CookieNameOptions>>().Value;
    }
}
