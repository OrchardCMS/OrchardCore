using OrchardCore.Modules.Manifest;
using OrchardCore.Users;

[assembly: Module(
    Name = "Users",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = UserConstants.Features.Users,
    Name = "Users",
    Description = "The users module enables authentication UI and user management.",
    Dependencies = new[] { "OrchardCore.Roles.Core" },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.ChangeEmail",
    Name = "Users Change Email",
    Description = "The Change email feature allows users to change their email address.",
    Dependencies = new[] { UserConstants.Features.Users },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.Registration",
    Name = "Users Registration",
    Description = "The registration feature allows external users to sign up to the site and ask to confirm their email.",
    Dependencies = new[]
    {
        UserConstants.Features.Users,
        "OrchardCore.Email",
    },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.ResetPassword",
    Name = "Users Reset Password",
    Description = "The reset password feature allows users to reset their password.",
    Dependencies = new[]
    {
        UserConstants.Features.Users,
        "OrchardCore.Email",
    },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.TimeZone",
    Name = "User Time Zone",
    Description = "Provides a way to set the time zone per user.",
    Dependencies = new[] { UserConstants.Features.Users },
    Category = "Settings"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.CustomUserSettings",
    Name = "Custom User Settings",
    Description = "The custom user settings feature allows content types to become custom user settings.",
    Dependencies = new[]
    {
        UserConstants.Features.Users,
        "OrchardCore.Contents",
    },
    Category = "Settings"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.AuditTrail",
    Name = "Users Audit Trail",
    Description = "The users audit trail feature allows logging of user events.",
    Dependencies = new[]
    {
        UserConstants.Features.Users,
        "OrchardCore.AuditTrail",
    },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.Authentication.CacheTicketStore",
    Name = "Users Authentication Ticket Store",
    Description = "Stores users authentication tickets on server in memory cache instead of cookies. If distributed cache feature is enabled it will store authentication tickets on distributed cache.",
    Dependencies = new[] { UserConstants.Features.Users },
    Category = "Security"
)]

[assembly: Feature(
    Id = UserConstants.Features.TwoFactorAuthentication,
    Name = "Two-Factor Authentication Services",
    Description = "Provices Two-factor core services.",
    Dependencies = new[] { UserConstants.Features.Users },
    EnabledByDependencyOnly = true,
    Category = "Security"
)]

[assembly: Feature(
    Id = UserConstants.Features.AuthenticatorApp,
    Name = "Two-Factor Authenticator App Method",
    Description = "Provides users a two-factor authentication method through any Authentication App.",
    Dependencies = new[]
    {
        UserConstants.Features.Users,
        UserConstants.Features.TwoFactorAuthentication,
    },
    Category = "Security"
)]

[assembly: Feature(
    Id = UserConstants.Features.EmailAuthenticator,
    Name = "Two-Factor Email Method",
    Description = "Provides users a two-factor authentication method through an Email service.",
    Dependencies = new[]
    {
        UserConstants.Features.Users,
        UserConstants.Features.TwoFactorAuthentication,
        "OrchardCore.Liquid",
        "OrchardCore.Email",
    },
    Category = "Security"
)]

[assembly: Feature(
    Id = UserConstants.Features.SmsAuthenticator,
    Name = "Two-Factor SMS Method",
    Description = "Provides users a two-factor authentication method through an SMS service.",
    Dependencies = new[]
    {
        UserConstants.Features.Users,
        UserConstants.Features.TwoFactorAuthentication,
        "OrchardCore.Liquid",
        "OrchardCore.Sms",
    },
    Category = "Security"
)]
