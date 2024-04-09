using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class TwoFactorEmailTokenProvider : IUserTwoFactorTokenProvider<IUser>
{
    private readonly Rfc6238AuthenticationService _service;
    private readonly TwoFactorEmailTokenProviderOptions _options;

    public TwoFactorEmailTokenProvider(
        IOptions<TwoFactorEmailTokenProviderOptions> options,
        IClock clock)
    {
        _options = options.Value;
        _service = new Rfc6238AuthenticationService(options.Value.TokenLifespan, options.Value.TokenLength, clock);
    }

    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<IUser> manager, IUser user)
        => Task.FromResult(manager is not null && user is not null);

    public async Task<string> GenerateAsync(string purpose, UserManager<IUser> manager, IUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var token = await manager.CreateSecurityTokenAsync(user);
        var modifier = await GetUserModifierAsync(purpose, manager, user);

        var pin = _service.GenerateCode(token, modifier);

        return _service.GetString(pin);
    }

    public async Task<bool> ValidateAsync(string purpose, string token, UserManager<IUser> manager, IUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (!int.TryParse(token, out var code))
        {
            return false;
        }

        var securityToken = await manager.CreateSecurityTokenAsync(user);
        var modifier = await GetUserModifierAsync(purpose, manager, user);

        return securityToken != null &&
            _service.ValidateCode(securityToken, code, modifier);
    }

    private static async Task<string> GetUserModifierAsync(string purpose, UserManager<IUser> manager, IUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var userId = await manager.GetUserIdAsync(user);

        return $"Totp:{purpose}:{userId}";
    }
}
