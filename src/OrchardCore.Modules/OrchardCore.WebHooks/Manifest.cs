using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "WebHooks",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0"
)]

[assembly: Feature(
    Id = "OrchardCore.WebHooks",
    Name = "WebHooks",
    Description = "Provides a way to notify external services about specified events.",
    Category = "Infrastructure",
    Dependencies = new[] { "OrchardCore.Liquid" }
)]
