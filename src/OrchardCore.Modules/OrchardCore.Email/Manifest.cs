using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Email",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Description = "Provides email settings configuration and a default email service based on SMTP.",
    Dependencies = new [] { "OrchardCore.Resources" },
    Category = "Messaging"
)]
