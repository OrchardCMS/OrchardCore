using OrchardCore.Cookies;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Users;

namespace OrchardCore.Tests.Shell;

public class CookieOptionsSetupTests
{
    [Fact]
    public void PostConfigure_NoConfiguration_KeepsDefaultCookieNames()
    {
        var services = CreateServices([]);

        var core = services.GetRequiredService<IOptions<CoreCookieOptions>>().Value;
        Assert.Equal("__orchantiforgery_v1", core.Antiforgery);
        Assert.Equal(".AspNetCore.Mvc.CookieTempDataProvider", core.TempData);
        Assert.Equal(".AspNet.Consent", core.Consent);

        var users = services.GetRequiredService<IOptions<UsersCookieOptions>>().Value;
        Assert.Equal("orchauth_My+Tenant", users.Authentication);
        Assert.Equal("orchemail_My+Tenant", users.EmailConfirmation);
        Assert.Equal("orchpasskey_My+Tenant", users.Passkey);
        Assert.Equal("Identity.External", users.ExternalAuthentication);
        Assert.Equal("Identity.TwoFactorUserId", users.TwoFactorUserId);
        Assert.Equal("Identity.TwoFactorRememberMe", users.TwoFactorRememberMe);
    }

    [Fact]
    public void PostConfigure_ConfiguredNames_UsesConfiguredNames()
    {
        var services = CreateServices(new Dictionary<string, string>
        {
            ["Cookies:Core:Antiforgery"] = "custom_antiforgery",
            ["Cookies:Core:TempData"] = " ",
            ["Cookies:Users:Authentication"] = "custom_auth",
        });

        var core = services.GetRequiredService<IOptions<CoreCookieOptions>>().Value;
        Assert.Equal("custom_antiforgery", core.Antiforgery);
        Assert.Equal(".AspNetCore.Mvc.CookieTempDataProvider", core.TempData);

        var users = services.GetRequiredService<IOptions<UsersCookieOptions>>().Value;
        Assert.Equal("custom_auth", users.Authentication);
        Assert.Equal("orchpasskey_My+Tenant", users.Passkey);
    }

    private static ServiceProvider CreateServices(Dictionary<string, string> values)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        return new ServiceCollection()
            .AddSingleton<IShellConfiguration>(new ShellConfiguration(configuration))
            .AddSingleton(new ShellSettings { Name = "My Tenant", VersionId = "v1" })
            .ConfigureOptions<CoreCookieOptionsSetup>()
            .ConfigureOptions<UsersCookieOptionsSetup>()
            .BuildServiceProvider();
    }
}
