using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Email",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "Provides email settings configuration and a default email service based on SMTP.",
    Dependencies = new[] { "OrchardCore.Resources" },
    Category = "Messaging"
)]
