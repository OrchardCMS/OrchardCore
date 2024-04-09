using System;
using System.Globalization;
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

    private string _format;

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

        return _service.GenerateCode(token, modifier)
            .ToString(GetStringFormat(), CultureInfo.InvariantCulture);
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

    private string GetStringFormat()
    {
        // Number of 0's is length of the generated pin.
        _format ??= _options.TokenLength switch
        {
            TwoFactorEmailTokenLength.Two => "D2",
            TwoFactorEmailTokenLength.Three => "D3",
            TwoFactorEmailTokenLength.Four => "D4",
            TwoFactorEmailTokenLength.Five => "D5",
            TwoFactorEmailTokenLength.Six => "D6",
            TwoFactorEmailTokenLength.Seven => "D7",
            TwoFactorEmailTokenLength.Eight or TwoFactorEmailTokenLength.Default => "D8",
            _ => throw new NotSupportedException("Unsupported token length.")
        };

        return _format;
    }
}
