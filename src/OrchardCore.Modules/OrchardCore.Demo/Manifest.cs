using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Orchard Demo",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "Test",
    Category = "Samples",
    Dependencies = new []{ "OrchardCore.Users", "OrchardCore.Contents" }
)]
