using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Publish Later",
    Author = "The Orchard Team",
    Website = "https://orchardcore.ne",
    Version = "1.0.0-rc2",
    Description = "The Publish Later module adds the ability to schedule content items to be published at a given future date and time.",
    Dependencies = new[] { "OrchardCore.Contents" },
    Category = "Content Management"
)]
