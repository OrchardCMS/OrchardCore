using OrchardCore.Modules.Manifest;
using OrchardCore.SignalR;

[assembly: Module(
    Name = "SignalR",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provides the services required to host and consume SignalR hubs.",
    Category = "Infrastructure"
)]

[assembly: Feature(
    Id = SignalRConstants.Feature.Area,
    Name = "SignalR",
    Description = "Registers SignalR, the SignalR client resources, and hub authentication.",
    Category = "Infrastructure"
)]
