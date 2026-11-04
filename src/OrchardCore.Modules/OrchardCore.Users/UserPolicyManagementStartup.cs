using Microsoft.Extensions.Localization;
using OrchardCore.Security.Services;
using OrchardCore.Liquid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Options;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.Services.Management;

namespace OrchardCore.Users;

/// <summary>Registers the feature-owned user-login management section.</summary>
[Feature("OrchardCore.Users")]
public sealed class LoginSettingsManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider => new UserPolicySectionProvider<LoginSettings>(
            provider.GetRequiredService<ISiteService>(), "user-login", "OrchardCore.Users",
            [
                UserPolicyField<LoginSettings>.Boolean("useSiteTheme", settings => settings.UseSiteTheme, (settings, value) => settings.UseSiteTheme = value),
                UserPolicyField<LoginSettings>.Boolean("disableLocalLogin", settings => settings.DisableLocalLogin, (settings, value) => settings.DisableLocalLogin = value),
                UserPolicyField<LoginSettings>.Boolean("allowChangingUsername", settings => settings.AllowChangingUsername, (settings, value) => settings.AllowChangingUsername = value),
                UserPolicyField<LoginSettings>.Boolean("allowChangingEmail", settings => settings.AllowChangingEmail, (settings, value) => settings.AllowChangingEmail = value),
                UserPolicyField<LoginSettings>.Boolean("allowRememberMe", settings => settings.AllowRememberMe, (settings, value) => settings.AllowRememberMe = value),
                UserPolicyField<LoginSettings>.Boolean("usePersistentAuthenticationCookie", settings => settings.UsePersistentAuthenticationCookie, (settings, value) => settings.UsePersistentAuthenticationCookie = value),
                UserPolicyField<LoginSettings>.Boolean("allowChangingPhoneNumber", settings => settings.AllowChangingPhoneNumber, (settings, value) => settings.AllowChangingPhoneNumber = value),
            ], UserPolicySettingsEditor.Clone, UserPolicySettingsEditor.Apply));
    }
}

/// <summary>Registers the feature-owned user-registration management section.</summary>
[Feature("OrchardCore.Users.Registration")]
public sealed class RegistrationSettingsManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider => new UserPolicySectionProvider<RegistrationSettings>(
            provider.GetRequiredService<ISiteService>(), "user-registration", "OrchardCore.Users.Registration",
            [
                UserPolicyField<RegistrationSettings>.Boolean("usersMustValidateEmail", settings => settings.UsersMustValidateEmail, (settings, value) => settings.UsersMustValidateEmail = value),
                UserPolicyField<RegistrationSettings>.Boolean("usersAreModerated", settings => settings.UsersAreModerated, (settings, value) => settings.UsersAreModerated = value),
                UserPolicyField<RegistrationSettings>.Boolean("useSiteTheme", settings => settings.UseSiteTheme, (settings, value) => settings.UseSiteTheme = value),
            ], UserPolicySettingsEditor.Clone, UserPolicySettingsEditor.Apply, () => provider.GetRequiredService<IOptionsUpdateNotifier>().RequestUpdate<RegistrationOptions>()));
    }
}

/// <summary>Registers the feature-owned user-password-reset management section.</summary>
[Feature("OrchardCore.Users.ResetPassword")]
public sealed class ResetPasswordSettingsManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider => new UserPolicySectionProvider<ResetPasswordSettings>(
            provider.GetRequiredService<ISiteService>(), "user-password-reset", "OrchardCore.Users.ResetPassword",
            [
                UserPolicyField<ResetPasswordSettings>.Boolean("allowResetPassword", settings => settings.AllowResetPassword, (settings, value) => settings.AllowResetPassword = value),
                UserPolicyField<ResetPasswordSettings>.Boolean("useSiteTheme", settings => settings.UseSiteTheme, (settings, value) => settings.UseSiteTheme = value),
            ], UserPolicySettingsEditor.Clone, UserPolicySettingsEditor.Apply));
    }
}

/// <summary>Registers the feature-owned user-change-email management section.</summary>
[Feature("OrchardCore.Users.ChangeEmail")]
public sealed class ChangeEmailSettingsManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider => new UserPolicySectionProvider<ChangeEmailSettings>(
            provider.GetRequiredService<ISiteService>(), "user-change-email", "OrchardCore.Users.ChangeEmail",
            [
                UserPolicyField<ChangeEmailSettings>.Boolean("allowChangeEmail", settings => settings.AllowChangeEmail, (settings, value) => settings.AllowChangeEmail = value),
            ], UserPolicySettingsEditor.Clone, UserPolicySettingsEditor.Apply));
    }
}

/// <summary>Registers the feature-owned user-two-factor management section.</summary>
[Feature("OrchardCore.Users.2FA")]
public sealed class TwoFactorLoginSettingsManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider => new UserPolicySectionProvider<TwoFactorLoginSettings>(
            provider.GetRequiredService<ISiteService>(), "user-two-factor", "OrchardCore.Users.2FA",
            [
                UserPolicyField<TwoFactorLoginSettings>.Boolean("requireTwoFactorAuthentication", settings => settings.RequireTwoFactorAuthentication, (settings, value) => settings.RequireTwoFactorAuthentication = value),
                UserPolicyField<TwoFactorLoginSettings>.Boolean("allowRememberClientTwoFactorAuthentication", settings => settings.AllowRememberClientTwoFactorAuthentication, (settings, value) => settings.AllowRememberClientTwoFactorAuthentication = value),
                UserPolicyField<TwoFactorLoginSettings>.Integer("numberOfRecoveryCodesToGenerate", 1, int.MaxValue, settings => settings.NumberOfRecoveryCodesToGenerate, (settings, value) => settings.NumberOfRecoveryCodesToGenerate = value),
                UserPolicyField<TwoFactorLoginSettings>.Boolean("useSiteTheme", settings => settings.UseSiteTheme, (settings, value) => settings.UseSiteTheme = value),
            ], UserPolicySettingsEditor.Clone, UserPolicySettingsEditor.Apply));
    }
}

/// <summary>Registers the feature-owned user-authenticator-app management section.</summary>
[Feature("OrchardCore.Users.2FA.AuthenticatorApp")]
[RequireFeatures(UserConstants.Features.TwoFactorAuthentication)]
public sealed class AuthenticatorAppLoginSettingsManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider => new UserPolicySectionProvider<AuthenticatorAppLoginSettings>(
            provider.GetRequiredService<ISiteService>(), "user-authenticator-app", "OrchardCore.Users.2FA.AuthenticatorApp",
            [
                UserPolicyField<AuthenticatorAppLoginSettings>.Boolean("useEmailAsAuthenticatorDisplayName", settings => settings.UseEmailAsAuthenticatorDisplayName, (settings, value) => settings.UseEmailAsAuthenticatorDisplayName = value),
                UserPolicyField<AuthenticatorAppLoginSettings>.Integer("tokenLength", 6, 6, settings => settings.TokenLength, (settings, value) => settings.TokenLength = value),
            ], UserPolicySettingsEditor.Clone, UserPolicySettingsEditor.Apply));
    }
}

/// <summary>Registers the feature-owned user-external-login management section.</summary>
[Feature("OrchardCore.Users.ExternalAuthentication")]
public sealed class ExternalLoginSettingsManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider => new UserPolicySectionProvider<ExternalLoginSettings>(
            provider.GetRequiredService<ISiteService>(), "user-external-login", "OrchardCore.Users.ExternalAuthentication",
            [
                UserPolicyField<ExternalLoginSettings>.Boolean("useExternalProviderIfOnlyOneDefined", settings => settings.UseExternalProviderIfOnlyOneDefined, (settings, value) => settings.UseExternalProviderIfOnlyOneDefined = value),
                UserPolicyField<ExternalLoginSettings>.Boolean("useScriptToSyncProperties", settings => settings.UseScriptToSyncProperties, (settings, value) => settings.UseScriptToSyncProperties = value),
                UserPolicyField<ExternalLoginSettings>.Text("syncPropertiesScript", settings => settings.SyncPropertiesScript, (settings, value) => settings.SyncPropertiesScript = value),
            ], UserPolicySettingsEditor.Clone, UserPolicySettingsEditor.Apply, () => provider.GetRequiredService<IOptionsUpdateNotifier>().RequestUpdate<ExternalLoginOptions>()));
    }
}

/// <summary>Registers the feature-owned user-external-registration management section.</summary>
[Feature("OrchardCore.Users.ExternalAuthentication")]
public sealed class ExternalRegistrationSettingsManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider => new UserPolicySectionProvider<ExternalRegistrationSettings>(
            provider.GetRequiredService<ISiteService>(), "user-external-registration", "OrchardCore.Users.ExternalAuthentication",
            [
                UserPolicyField<ExternalRegistrationSettings>.Boolean("disableNewRegistrations", settings => settings.DisableNewRegistrations, (settings, value) => settings.DisableNewRegistrations = value),
                UserPolicyField<ExternalRegistrationSettings>.Boolean("noPassword", settings => settings.NoPassword, (settings, value) => settings.NoPassword = value),
                UserPolicyField<ExternalRegistrationSettings>.Boolean("noUsername", settings => settings.NoUsername, (settings, value) => settings.NoUsername = value),
                UserPolicyField<ExternalRegistrationSettings>.Boolean("noEmail", settings => settings.NoEmail, (settings, value) => settings.NoEmail = value),
                UserPolicyField<ExternalRegistrationSettings>.Boolean("useScriptToGenerateUsername", settings => settings.UseScriptToGenerateUsername, (settings, value) => settings.UseScriptToGenerateUsername = value),
                UserPolicyField<ExternalRegistrationSettings>.Text("generateUsernameScript", settings => settings.GenerateUsernameScript, (settings, value) => settings.GenerateUsernameScript = value),
            ], UserPolicySettingsEditor.Clone, UserPolicySettingsEditor.Apply));
    }
}

/// <summary>Registers the feature-owned user-email-authenticator management section.</summary>
[Feature("OrchardCore.Users.2FA.Email")]
public sealed class EmailAuthenticatorLoginSettingsManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider => new UserPolicySectionProvider<EmailAuthenticatorLoginSettings>(
            provider.GetRequiredService<ISiteService>(), "user-email-authenticator", "OrchardCore.Users.2FA.Email",
            [
                UserPolicyField<EmailAuthenticatorLoginSettings>.Text("subject", settings => settings.Subject, (settings, value) => settings.Subject = value),
                UserPolicyField<EmailAuthenticatorLoginSettings>.Text("body", settings => settings.Body, (settings, value) => settings.Body = value),
            ], UserPolicySettingsEditor.Clone, UserPolicySettingsEditor.Apply, validate: settings => Task.FromResult(UserPolicySettingsEditor.Validate(settings, provider.GetRequiredService<ILiquidTemplateManager>()))));
    }
}

/// <summary>Registers the feature-owned user-sms-authenticator management section.</summary>
[Feature("OrchardCore.Users.2FA.Sms")]
public sealed class SmsAuthenticatorLoginSettingsManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider => new UserPolicySectionProvider<SmsAuthenticatorLoginSettings>(
            provider.GetRequiredService<ISiteService>(), "user-sms-authenticator", "OrchardCore.Users.2FA.Sms",
            [
                UserPolicyField<SmsAuthenticatorLoginSettings>.Text("body", settings => settings.Body, (settings, value) => settings.Body = value),
            ], UserPolicySettingsEditor.Clone, UserPolicySettingsEditor.Apply, validate: settings => Task.FromResult(UserPolicySettingsEditor.Validate(settings, provider.GetRequiredService<ILiquidTemplateManager>()))));
    }
}

/// <summary>Registers the role-specific MFA policy when roles and MFA are enabled.</summary>
[RequireFeatures("OrchardCore.Roles", UserConstants.Features.TwoFactorAuthentication)]
public sealed class RoleLoginSettingsManagementStartup : StartupBase
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider>(provider => new UserPolicySectionProvider<RoleLoginSettings>(
            provider.GetRequiredService<ISiteService>(), "user-role-two-factor", UserConstants.Features.TwoFactorAuthentication,
            [
                UserPolicyField<RoleLoginSettings>.Boolean("requireTwoFactorAuthenticationForSpecificRoles", settings => settings.RequireTwoFactorAuthenticationForSpecificRoles,
                    (settings, value) => settings.RequireTwoFactorAuthenticationForSpecificRoles = value),
                UserPolicyField<RoleLoginSettings>.Names("roles", settings => settings.Roles, (settings, value) => settings.Roles = value),
            ], UserPolicySettingsEditor.Clone, UserPolicySettingsEditor.Apply,
            validate: settings => UserPolicySettingsEditor.ValidateAsync(settings, provider.GetRequiredService<IRoleService>(), provider.GetRequiredService<IStringLocalizer<RoleLoginSettingsManagementStartup>>())));
    }
}
