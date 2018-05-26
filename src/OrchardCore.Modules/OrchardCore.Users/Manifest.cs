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
    Description = "The users feature enables authentication UI and user management.",
    Dependencies = new [] { "OrchardCore.Authentication", "OrchardCore.DataProtection", "OrchardCore.Email" },
    Category = "Security"
)]


[assembly: Feature(
    Id = "OrchardCore.Users.TimeZone",
    Name = "User Time Zone",
    Description = "Provides a way to set the time zone per user.",
    Category = "Settings"
)]