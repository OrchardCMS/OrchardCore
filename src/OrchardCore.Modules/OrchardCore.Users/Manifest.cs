using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Users",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The users module enables authentication UI and user management.",
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users",
    Name = "Users",
    Description = "The Workflows module provides tools and APIs to create custom workflows",
    Dependencies = new[] { "OrchardCore.Authentication", "OrchardCore.DataProtection", "OrchardCore.Email" },
    Category = "Security"
)]

[assembly: Feature(
    Id = "OrchardCore.Users.TimeZone",
    Name = "Users Time Zone",
    Description = "Set the time zone per user account",
    Category = "Settings"
)]