using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "SignalR",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides the services required to host and consume SignalR hubs.",
    Category = "Infrastructure"
)]

[assembly: Feature(
    Id = "OrchardCore.SignalR",
    Name = "SignalR",
    Description = "Registers SignalR and the SignalR client resources.",
    Category = "Infrastructure"
)]
