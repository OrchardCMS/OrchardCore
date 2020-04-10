using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Remote Deployment",
    Author = "The Orchard Team",
    Website = "https://orchardcore.net",
    Version = "1.0.0-rc2",
    Description = "Provide the ability to export and import to and from a remote server.",
    Dependencies = new[] { "OrchardCore.Deployment" },
    Category = "Deployment"
)]
