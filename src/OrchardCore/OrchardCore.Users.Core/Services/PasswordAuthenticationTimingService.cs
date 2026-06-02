using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class PasswordAuthenticationTimingService : IPasswordAuthenticationTimingService
{
    // These are intentionally just a bounded jitter window, not protocol-defined magic numbers.
    // The goal is to add enough variance on failed password authentication to make username-based
    // timing differences harder to measure, without adding an excessive delay that would noticeably
    // degrade the user experience for normal failed logins.
    private static readonly TimeSpan _minimumDelay = TimeSpan.FromMilliseconds(175);
    private static readonly TimeSpan _maximumDelay = TimeSpan.FromMilliseconds(425);

    private readonly IPasswordHasher<IUser> _passwordHasher;
    private readonly User _sentinelUser = new();
    private readonly string _sentinelPasswordHash;

    public PasswordAuthenticationTimingService(IPasswordHasher<IUser> passwordHasher)
    {
        _passwordHasher = passwordHasher;
        _sentinelPasswordHash = _passwordHasher.HashPassword(_sentinelUser, Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
    }

    public async Task MitigateUnknownUserAsync(string password, CancellationToken cancellationToken = default)
    {
        _ = _passwordHasher.VerifyHashedPassword(_sentinelUser, _sentinelPasswordHash, password ?? string.Empty);

        await DelayFailedAuthenticationAsync(cancellationToken);
    }

    public Task DelayFailedAuthenticationAsync(CancellationToken cancellationToken = default)
        => Task.Delay(GetRandomDelay(), cancellationToken);

    private static TimeSpan GetRandomDelay()
        => TimeSpan.FromMilliseconds(RandomNumberGenerator.GetInt32((int)_minimumDelay.TotalMilliseconds, (int)_maximumDelay.TotalMilliseconds + 1));
}
