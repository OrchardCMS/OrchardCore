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

[assembly: Feature(
    Id = SignalRConstants.Feature.RedisBackplane,
    Name = "SignalR Redis Backplane",
    Description = "Uses Redis as the SignalR backplane, enabling multi-instance deployments with a tenant-qualified channel prefix.",
    Category = "Infrastructure",
    Dependencies =
    [
        SignalRConstants.Feature.Area,
        "OrchardCore.Redis",
    ]
)]

[assembly: Feature(
    Id = SignalRConstants.Feature.AzureBackplane,
    Name = "SignalR Azure Backplane",
    Description = "Uses the Azure SignalR Service as the SignalR backplane, enabling multi-instance deployments.",
    Category = "Infrastructure",
    Dependencies =
    [
        SignalRConstants.Feature.Area,
    ]
)]
