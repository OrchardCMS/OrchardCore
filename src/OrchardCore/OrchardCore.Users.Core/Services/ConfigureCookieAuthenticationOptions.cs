using System.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Users.Services;

public class ConfigureCookieAuthenticationOptions : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly string _tenantName;

    public ConfigureCookieAuthenticationOptions(ShellSettings shellSettings)
    {
        _tenantName = shellSettings.Name;
    }

    public void PostConfigure(string name, CookieAuthenticationOptions options)
    {
        var userOptions = ShellScope.Services.GetRequiredService<IOptions<UserOptions>>();

        options.Cookie.Name = "orchauth_" + HttpUtility.UrlEncode(_tenantName);

        // Don't set the cookie builder 'Path' so that it uses the 'IAuthenticationFeature' value
        // set by the pipeline and coming from the request 'PathBase' which already ends with the
        // tenant prefix but may also start by a path related e.g to a virtual folder.

        options.LoginPath = "/" + userOptions.Value.LoginPath;
        options.LogoutPath = "/" + userOptions.Value.LogoffPath;
        options.AccessDeniedPath = "/Error/403";
    }
}
