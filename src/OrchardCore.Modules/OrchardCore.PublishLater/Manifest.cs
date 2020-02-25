using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Publish Later",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0",
    Description = "The Publish Later module adds the ability to schedule content items to be published at a given future date and time.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
