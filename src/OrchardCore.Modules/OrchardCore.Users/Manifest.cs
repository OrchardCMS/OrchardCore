using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Users",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "The users module enables authentication UI and user management.",
    Dependencies = new [] { "OrchardCore.Authentication", "OrchardCore.DataProtection", "OrchardCore.Email" },
    Category = "Security"
)]
