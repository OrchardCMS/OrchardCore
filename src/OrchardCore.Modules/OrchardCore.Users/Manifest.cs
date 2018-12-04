using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Users",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.Users",
    Name = "Users",
    Description = "The users module enables authentication UI and user management.",
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
    Category = "Settings"
)]