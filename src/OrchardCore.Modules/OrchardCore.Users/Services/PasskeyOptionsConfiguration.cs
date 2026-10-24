using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Users.Services;

/// <summary>
/// Binds the <see cref="IdentityPasskeyOptions"/> from the <c>OrchardCore:Users:Passkeys</c> configuration section.
/// </summary>
public sealed class PasskeyOptionsConfiguration : IConfigureOptions<IdentityPasskeyOptions>
{
    private readonly IShellConfiguration _shellConfiguration;

    public PasskeyOptionsConfiguration(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(IdentityPasskeyOptions options)
    {
        _shellConfiguration.GetSection("Users:Passkeys").Bind(options);

        // By default, the origin of the page is compared with the 'Origin' header of the request posting the credential,
        // which browsers don't always send with a form, so compare it with the origin of the request instead.
        options.ValidateOrigin ??= context => ValueTask.FromResult(IsRequestOrigin(context));
    }

    private static bool IsRequestOrigin(PasskeyOriginValidationContext context)
    {
        if (context.CrossOrigin || !Uri.TryCreate(context.Origin, UriKind.Absolute, out var origin))
        {
            return false;
        }

        var request = context.HttpContext.Request;

        return Uri.TryCreate($"{request.Scheme}://{request.Host}", UriKind.Absolute, out var requestOrigin) &&
            origin == requestOrigin;
    }
}
