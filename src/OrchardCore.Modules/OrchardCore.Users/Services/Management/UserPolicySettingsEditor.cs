using Microsoft.Extensions.Localization;
using OrchardCore.Security.Services;
using OrchardCore.Liquid;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services.Management;

internal static class UserPolicySettingsEditor
{
    public static LoginSettings Clone(LoginSettings settings) => new()
    {
        UseSiteTheme = settings.UseSiteTheme,
        DisableLocalLogin = settings.DisableLocalLogin,
        AllowChangingUsername = settings.AllowChangingUsername,
        AllowChangingEmail = settings.AllowChangingEmail,
        AllowRememberMe = settings.AllowRememberMe,
        UsePersistentAuthenticationCookie = settings.UsePersistentAuthenticationCookie,
        AllowChangingPhoneNumber = settings.AllowChangingPhoneNumber,
    };

    public static bool Apply(LoginSettings current, LoginSettings proposed)
    {
        var changed = current.UseSiteTheme != proposed.UseSiteTheme
            || current.DisableLocalLogin != proposed.DisableLocalLogin
            || current.AllowChangingUsername != proposed.AllowChangingUsername
            || current.AllowChangingEmail != proposed.AllowChangingEmail
            || current.AllowRememberMe != proposed.AllowRememberMe
            || current.UsePersistentAuthenticationCookie != proposed.UsePersistentAuthenticationCookie
            || current.AllowChangingPhoneNumber != proposed.AllowChangingPhoneNumber;
        current.UseSiteTheme = proposed.UseSiteTheme;
        current.DisableLocalLogin = proposed.DisableLocalLogin;
        current.AllowChangingUsername = proposed.AllowChangingUsername;
        current.AllowChangingEmail = proposed.AllowChangingEmail;
        current.AllowRememberMe = proposed.AllowRememberMe;
        current.UsePersistentAuthenticationCookie = proposed.UsePersistentAuthenticationCookie;
        current.AllowChangingPhoneNumber = proposed.AllowChangingPhoneNumber;
        return changed;
    }

    public static RegistrationSettings Clone(RegistrationSettings settings) => new()
    {
        UsersMustValidateEmail = settings.UsersMustValidateEmail,
        UsersAreModerated = settings.UsersAreModerated,
        UseSiteTheme = settings.UseSiteTheme,
    };

    public static bool Apply(RegistrationSettings current, RegistrationSettings proposed)
    {
        var changed = current.UsersMustValidateEmail != proposed.UsersMustValidateEmail
            || current.UsersAreModerated != proposed.UsersAreModerated
            || current.UseSiteTheme != proposed.UseSiteTheme;
        current.UsersMustValidateEmail = proposed.UsersMustValidateEmail;
        current.UsersAreModerated = proposed.UsersAreModerated;
        current.UseSiteTheme = proposed.UseSiteTheme;
        return changed;
    }

    public static ResetPasswordSettings Clone(ResetPasswordSettings settings) => new()
    {
        AllowResetPassword = settings.AllowResetPassword,
        UseSiteTheme = settings.UseSiteTheme,
    };

    public static bool Apply(ResetPasswordSettings current, ResetPasswordSettings proposed)
    {
        var changed = current.AllowResetPassword != proposed.AllowResetPassword
            || current.UseSiteTheme != proposed.UseSiteTheme;
        current.AllowResetPassword = proposed.AllowResetPassword;
        current.UseSiteTheme = proposed.UseSiteTheme;
        return changed;
    }

    public static ChangeEmailSettings Clone(ChangeEmailSettings settings) => new()
    {
        AllowChangeEmail = settings.AllowChangeEmail,
    };

    public static bool Apply(ChangeEmailSettings current, ChangeEmailSettings proposed)
    {
        var changed = current.AllowChangeEmail != proposed.AllowChangeEmail;
        current.AllowChangeEmail = proposed.AllowChangeEmail;
        return changed;
    }

    public static TwoFactorLoginSettings Clone(TwoFactorLoginSettings settings) => new()
    {
        RequireTwoFactorAuthentication = settings.RequireTwoFactorAuthentication,
        AllowRememberClientTwoFactorAuthentication = settings.AllowRememberClientTwoFactorAuthentication,
        NumberOfRecoveryCodesToGenerate = settings.NumberOfRecoveryCodesToGenerate,
        UseSiteTheme = settings.UseSiteTheme,
    };

    public static bool Apply(TwoFactorLoginSettings current, TwoFactorLoginSettings proposed)
    {
        if (proposed.NumberOfRecoveryCodesToGenerate < 1) { return false; }
        var changed = current.RequireTwoFactorAuthentication != proposed.RequireTwoFactorAuthentication
            || current.AllowRememberClientTwoFactorAuthentication != proposed.AllowRememberClientTwoFactorAuthentication
            || current.NumberOfRecoveryCodesToGenerate != proposed.NumberOfRecoveryCodesToGenerate
            || current.UseSiteTheme != proposed.UseSiteTheme;
        current.RequireTwoFactorAuthentication = proposed.RequireTwoFactorAuthentication;
        current.AllowRememberClientTwoFactorAuthentication = proposed.AllowRememberClientTwoFactorAuthentication;
        current.NumberOfRecoveryCodesToGenerate = proposed.NumberOfRecoveryCodesToGenerate;
        current.UseSiteTheme = proposed.UseSiteTheme;
        return changed;
    }

    public static AuthenticatorAppLoginSettings Clone(AuthenticatorAppLoginSettings settings) => new()
    {
        UseEmailAsAuthenticatorDisplayName = settings.UseEmailAsAuthenticatorDisplayName,
        TokenLength = settings.TokenLength,
    };

    public static bool Apply(AuthenticatorAppLoginSettings current, AuthenticatorAppLoginSettings proposed)
    {
        if (proposed.TokenLength != 6) { return false; }
        var changed = current.UseEmailAsAuthenticatorDisplayName != proposed.UseEmailAsAuthenticatorDisplayName
            || current.TokenLength != proposed.TokenLength;
        current.UseEmailAsAuthenticatorDisplayName = proposed.UseEmailAsAuthenticatorDisplayName;
        current.TokenLength = proposed.TokenLength;
        return changed;
    }

    public static ExternalLoginSettings Clone(ExternalLoginSettings settings) => new()
    {
        UseExternalProviderIfOnlyOneDefined = settings.UseExternalProviderIfOnlyOneDefined,
        UseScriptToSyncProperties = settings.UseScriptToSyncProperties,
        SyncPropertiesScript = settings.SyncPropertiesScript,
    };

    public static bool Apply(ExternalLoginSettings current, ExternalLoginSettings proposed)
    {
        var changed = current.UseExternalProviderIfOnlyOneDefined != proposed.UseExternalProviderIfOnlyOneDefined
            || current.UseScriptToSyncProperties != proposed.UseScriptToSyncProperties
            || current.SyncPropertiesScript != proposed.SyncPropertiesScript;
        current.UseExternalProviderIfOnlyOneDefined = proposed.UseExternalProviderIfOnlyOneDefined;
        current.UseScriptToSyncProperties = proposed.UseScriptToSyncProperties;
        current.SyncPropertiesScript = proposed.SyncPropertiesScript;
        return changed;
    }

    public static ExternalRegistrationSettings Clone(ExternalRegistrationSettings settings) => new()
    {
        DisableNewRegistrations = settings.DisableNewRegistrations,
        NoPassword = settings.NoPassword,
        NoUsername = settings.NoUsername,
        NoEmail = settings.NoEmail,
        UseScriptToGenerateUsername = settings.UseScriptToGenerateUsername,
        GenerateUsernameScript = settings.GenerateUsernameScript,
    };

    public static bool Apply(ExternalRegistrationSettings current, ExternalRegistrationSettings proposed)
    {
        var changed = current.DisableNewRegistrations != proposed.DisableNewRegistrations
            || current.NoPassword != proposed.NoPassword
            || current.NoUsername != proposed.NoUsername
            || current.NoEmail != proposed.NoEmail
            || current.UseScriptToGenerateUsername != proposed.UseScriptToGenerateUsername
            || current.GenerateUsernameScript != proposed.GenerateUsernameScript;
        current.DisableNewRegistrations = proposed.DisableNewRegistrations;
        current.NoPassword = proposed.NoPassword;
        current.NoUsername = proposed.NoUsername;
        current.NoEmail = proposed.NoEmail;
        current.UseScriptToGenerateUsername = proposed.UseScriptToGenerateUsername;
        current.GenerateUsernameScript = proposed.GenerateUsernameScript;
        return changed;
    }

    public static EmailAuthenticatorLoginSettings Clone(EmailAuthenticatorLoginSettings settings) => new()
    {
        Subject = settings.Subject,
        Body = settings.Body,
    };

    public static bool Apply(EmailAuthenticatorLoginSettings current, EmailAuthenticatorLoginSettings proposed)
    {
        var changed = current.Subject != proposed.Subject
            || current.Body != proposed.Body;
        current.Subject = proposed.Subject;
        current.Body = proposed.Body;
        return changed;
    }

    public static SmsAuthenticatorLoginSettings Clone(SmsAuthenticatorLoginSettings settings) => new()
    {
        Body = settings.Body,
    };

    public static bool Apply(SmsAuthenticatorLoginSettings current, SmsAuthenticatorLoginSettings proposed)
    {
        var changed = current.Body != proposed.Body;
        current.Body = proposed.Body;
        return changed;
    }

    public static Dictionary<string, string[]> Validate(EmailAuthenticatorLoginSettings settings, ILiquidTemplateManager liquid)
    {
        var errors = new Dictionary<string, string[]>();
        if (!liquid.Validate(settings.Subject ?? string.Empty, out var subjectErrors)) { errors[nameof(settings.Subject)] = subjectErrors.ToArray(); }
        if (!liquid.Validate(settings.Body ?? string.Empty, out var bodyErrors)) { errors[nameof(settings.Body)] = bodyErrors.ToArray(); }
        return errors;
    }

    public static Dictionary<string, string[]> Validate(SmsAuthenticatorLoginSettings settings, ILiquidTemplateManager liquid)
    {
        var errors = new Dictionary<string, string[]>();
        if (!liquid.Validate(settings.Body ?? string.Empty, out var bodyErrors)) { errors[nameof(settings.Body)] = bodyErrors.ToArray(); }
        return errors;
    }
    public static RoleLoginSettings Clone(RoleLoginSettings settings) => new()
    {
        RequireTwoFactorAuthenticationForSpecificRoles = settings.RequireTwoFactorAuthenticationForSpecificRoles,
        Roles = settings.Roles?.ToArray(),
    };

    public static bool Apply(RoleLoginSettings current, RoleLoginSettings proposed)
    {
        var changed = current.RequireTwoFactorAuthenticationForSpecificRoles != proposed.RequireTwoFactorAuthenticationForSpecificRoles
            || !(current.Roles ?? []).SequenceEqual(proposed.Roles ?? [], StringComparer.Ordinal);
        current.RequireTwoFactorAuthenticationForSpecificRoles = proposed.RequireTwoFactorAuthenticationForSpecificRoles;
        current.Roles = proposed.Roles?.ToArray();
        return changed;
    }

    public static async Task<Dictionary<string, string[]>> ValidateAsync(RoleLoginSettings settings, IRoleService roles, IStringLocalizer localizer = null)
    {
        var errors = new Dictionary<string, string[]>();
        // Preserve inactive selections so a deleted role cannot prevent disabling MFA policy.
        if (!settings.RequireTwoFactorAuthenticationForSpecificRoles)
        {
            return errors;
        }
        var available = (await roles.GetAssignableRolesAsync()).Select(role => role.RoleName).ToArray();
        var selected = settings.Roles ?? [];
        if (selected.Any(name => !available.Contains(name, StringComparer.OrdinalIgnoreCase)))
        {
            errors[nameof(settings.Roles)] = [localizer?["Select existing assignable roles."].Value ?? "Select existing assignable roles."];
        }
        else
        {
            settings.Roles = selected.Select(name => available.First(role => string.Equals(role, name, StringComparison.OrdinalIgnoreCase)))
                .Distinct(StringComparer.Ordinal).ToArray();
        }
        if (settings.RequireTwoFactorAuthenticationForSpecificRoles && selected.Length == 0)
        {
            errors[nameof(settings.Roles)] = [localizer?["Select at least one role."].Value ?? "Select at least one role."];
        }
        return errors;
    }
}
