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
    Dependencies = new[] { "OrchardCore.Authentication", "OrchardCore.DataProtection" },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.Password",
    Name = "Users Password",
    Description = "The password feature allows to enable an option to send an email to a user to reset a forgotten password.",
    Dependencies = new[] { "OrchardCore.Users", "OrchardCore.Email" },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.Registration",
    Name = "Users Registration",
    Description = "The registration feature allows to enable an option to authorize external users to sign up to the site.",
    Dependencies = new[] { "OrchardCore.Users", "OrchardCore.Email" },
    Category = "Security"
)]