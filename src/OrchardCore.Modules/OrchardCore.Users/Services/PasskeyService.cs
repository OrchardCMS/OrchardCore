using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Modules;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

/// <summary>
/// Registers passkeys and verifies passkey assertions.
/// </summary>
/// <remarks>
/// The <see cref="SignInManager{TUser}"/> passkey methods keep the ceremony state in the two-factor user cookie,
/// which would replace the user of a two-factor sign-in that is underway. This service keeps it in a cookie of its own
/// instead, so a passkey can also be used as the second factor.
/// </remarks>
public sealed class PasskeyService
{
    private const string OperationKey = "PasskeyOperation";
    private const string StateKey = "PasskeyState";
    private const string AttestationOperation = "Attestation";
    private const string AssertionOperation = "Assertion";

    private readonly IPasskeyHandler<IUser> _passkeyHandler;
    private readonly UserManager<IUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IClock _clock;

    public PasskeyService(
        IPasskeyHandler<IUser> passkeyHandler,
        UserManager<IUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IClock clock)
    {
        _passkeyHandler = passkeyHandler;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _clock = clock;
    }

    /// <summary>
    /// Creates the options to pass to <c>navigator.credentials.create()</c> to register a passkey for a user.
    /// </summary>
    /// <param name="user">The user registering the passkey.</param>
    /// <returns>The creation options, as JSON.</returns>
    public async Task<string> MakeCreationOptionsAsync(IUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var userName = await _userManager.GetUserNameAsync(user);

        var userEntity = new PasskeyUserEntity
        {
            Id = await _userManager.GetUserIdAsync(user),
            Name = userName,
            DisplayName = userName,
        };

        var result = await _passkeyHandler.MakeCreationOptionsAsync(userEntity, HttpContext);

        await StoreStateAsync(AttestationOperation, result.AttestationState);

        return result.CreationOptionsJson;
    }

    /// <summary>
    /// Verifies the credential returned by <c>navigator.credentials.create()</c>.
    /// </summary>
    /// <param name="credentialJson">The credential, serialized as JSON.</param>
    public async Task<PasskeyAttestationResult> PerformAttestationAsync(string credentialJson)
    {
        var state = await RetrieveStateAsync(AttestationOperation);

        if (state == null)
        {
            return PasskeyAttestationResult.Fail(new PasskeyException("No passkey registration is underway."));
        }

        if (string.IsNullOrEmpty(credentialJson))
        {
            return PasskeyAttestationResult.Fail(new PasskeyException("No passkey credential was provided."));
        }

        return await _passkeyHandler.PerformAttestationAsync(new PasskeyAttestationContext
        {
            HttpContext = HttpContext,
            CredentialJson = credentialJson,
            AttestationState = state,
        });
    }

    /// <summary>
    /// Creates the options to pass to <c>navigator.credentials.get()</c> to sign in with a passkey.
    /// </summary>
    /// <param name="user">
    /// The user who must own the passkey, or <see langword="null"/> to let the browser offer any passkey of the site.
    /// </param>
    /// <returns>The request options, as JSON.</returns>
    public async Task<string> MakeRequestOptionsAsync(IUser user)
    {
        var result = await _passkeyHandler.MakeRequestOptionsAsync(user, HttpContext);

        await StoreStateAsync(AssertionOperation, result.AssertionState);

        return result.RequestOptionsJson;
    }

    /// <summary>
    /// Verifies the credential returned by <c>navigator.credentials.get()</c>.
    /// </summary>
    /// <param name="credentialJson">The credential, serialized as JSON.</param>
    public async Task<PasskeyAssertionResult<IUser>> PerformAssertionAsync(string credentialJson)
    {
        var state = await RetrieveStateAsync(AssertionOperation);

        if (state == null)
        {
            return PasskeyAssertionResult.Fail<IUser>(new PasskeyException("No passkey sign-in is underway."));
        }

        if (string.IsNullOrEmpty(credentialJson))
        {
            return PasskeyAssertionResult.Fail<IUser>(new PasskeyException("No passkey credential was provided."));
        }

        return await _passkeyHandler.PerformAssertionAsync(new PasskeyAssertionContext
        {
            HttpContext = HttpContext,
            CredentialJson = credentialJson,
            AssertionState = state,
        });
    }

    /// <summary>
    /// Saves a passkey after it was used to sign in, with its new signature counter and the time it was used.
    /// </summary>
    /// <param name="user">The user who signed in.</param>
    /// <param name="passkey">The passkey returned by <see cref="PerformAssertionAsync(string)"/>.</param>
    public Task<IdentityResult> UpdateUsedPasskeyAsync(IUser user, UserPasskeyInfo passkey)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(passkey);

        if (user is User u)
        {
            var userPasskey = u.Passkeys.FirstOrDefault(x => x.CredentialId.AsSpan().SequenceEqual(passkey.CredentialId));

            if (userPasskey != null)
            {
                userPasskey.LastUsedUtc = _clock.UtcNow;
            }
        }

        return _userManager.AddOrUpdatePasskeyAsync(user, passkey);
    }

    private HttpContext HttpContext
        => _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("Passkey operations require an HTTP context.");

    private Task StoreStateAsync(string operation, string state)
    {
        var properties = new AuthenticationProperties();
        properties.Items[OperationKey] = operation;
        properties.Items[StateKey] = state;

        var principal = new ClaimsPrincipal(new ClaimsIdentity(UserConstants.PasskeyAuthenticationScheme));

        return HttpContext.SignInAsync(UserConstants.PasskeyAuthenticationScheme, principal, properties);
    }

    private async Task<string> RetrieveStateAsync(string operation)
    {
        var result = await HttpContext.AuthenticateAsync(UserConstants.PasskeyAuthenticationScheme);

        // A ceremony state can only be used once.
        await HttpContext.SignOutAsync(UserConstants.PasskeyAuthenticationScheme);

        if (result.Properties == null ||
            !result.Properties.Items.TryGetValue(OperationKey, out var storedOperation) ||
            !string.Equals(storedOperation, operation, StringComparison.Ordinal) ||
            !result.Properties.Items.TryGetValue(StateKey, out var state))
        {
            return null;
        }

        return state;
    }
}
