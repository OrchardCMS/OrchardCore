using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users.Services;

/// <summary>
/// Provides timing normalization for password-based authentication flows.
/// When a user lookup returns null (username doesn't exist), calling
/// <see cref="NormalizeResponseTime"/> performs a dummy bcrypt hash
/// verification so the response time is indistinguishable from a real
/// password check. This prevents username enumeration via timing analysis.
/// </summary>
public sealed class PasswordTimingNormalizationService
{
    // A pre-computed bcrypt hash of an arbitrary string.
    // The actual password doesn't matter — the goal is to spend the same
    // CPU time as a real VerifyHashedPassword call.
    private static readonly string _dummyHash =
        new PasswordHasher<object>().HashPassword(null, "OrchardCore-DummyPassword-TimingNormalization");

    private readonly PasswordHasher<object> _hasher = new();

    /// <summary>
    /// Performs a dummy password-hash verification to normalize response
    /// timing when the requested user does not exist. Call this in the
    /// code path where user lookup returned null before returning an error.
    /// </summary>
    public void NormalizeResponseTime()
    {
        // The result is intentionally discarded — we only care about
        // consuming the same CPU time as a real bcrypt verification.
        _hasher.VerifyHashedPassword(null, _dummyHash, "wrong-password");
    }
}
