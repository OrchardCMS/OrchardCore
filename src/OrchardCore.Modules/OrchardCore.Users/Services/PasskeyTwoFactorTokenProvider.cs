using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Users.Services;

/// <summary>
/// A two-factor provider that verifies the second factor with one of the user's passkeys.
/// </summary>
/// <remarks>
/// The token is the credential returned by <c>navigator.credentials.get()</c>, serialized as JSON,
/// so there is no code to generate or send.
/// </remarks>
public sealed class PasskeyTwoFactorTokenProvider : IUserTwoFactorTokenProvider<IUser>
{
    private readonly IServiceProvider _serviceProvider;

    public PasskeyTwoFactorTokenProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<IUser> manager, IUser user)
    {
        return (await manager.GetPasskeysAsync(user)).Count > 0;
    }

    public Task<string> GenerateAsync(string purpose, UserManager<IUser> manager, IUser user)
    {
        return Task.FromResult(string.Empty);
    }

    public async Task<bool> ValidateAsync(string purpose, string token, UserManager<IUser> manager, IUser user)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        // Resolved lazily, as the 'UserManager' that creates this provider is itself a dependency of the 'PasskeyService'.
        var passkeyService = _serviceProvider.GetRequiredService<PasskeyService>();

        var result = await passkeyService.PerformAssertionAsync(token);

        if (!result.Succeeded ||
            !string.Equals(await manager.GetUserIdAsync(result.User), await manager.GetUserIdAsync(user), StringComparison.Ordinal))
        {
            return false;
        }

        return (await passkeyService.UpdateUsedPasskeyAsync(user, result.Passkey)).Succeeded;
    }
}
