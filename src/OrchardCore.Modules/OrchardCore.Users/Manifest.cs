using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Users",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Users",
    Name = "Users",
    Description = "The users module enables authentication UI and user management.",
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.ChangeEmail",
    Name = "Users Change Email",
    Description = "The Change email feature allows users to change their email address.",
    Dependencies = new[] { "OrchardCore.Users" },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.Registration",
    Name = "Users Registration",
    Description = "The registration feature allows external users to sign up to the site and ask to confirm their email.",
    Dependencies = new[] { "OrchardCore.Users", "OrchardCore.Email" },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.ResetPassword",
    Name = "Users Reset Password",
    Description = "The reset password feature allows users to reset their password.",
    Dependencies = new[] { "OrchardCore.Users", "OrchardCore.Email" },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.TimeZone",
    Name = "User Time Zone",
    Description = "Provides a way to set the time zone per user.",
    Dependencies = new[] { "OrchardCore.Users" },
    Category = "Settings"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.CustomUserSettings",
    Name = "Custom User Settings",
    Description = "The custom user settings feature allows content types to become custom user settings.",
    Dependencies = new[] { "OrchardCore.Contents", "OrchardCore.Users" },
    Category = "Settings"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.AuditTrail",
    Name = "Users Audit Trail",
    Description = "The users audit trail feature allows logging of user events.",
    Dependencies = new[] { "OrchardCore.AuditTrail", "OrchardCore.Users" },
    Category = "Security"
)]
