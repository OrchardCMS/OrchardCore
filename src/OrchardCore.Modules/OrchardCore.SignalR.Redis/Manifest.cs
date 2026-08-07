using OrchardCore.Modules.Manifest;
using OrchardCore.SignalR;

[assembly: Module(
    Name = "SignalR Redis Backplane",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Routes SignalR messages across application nodes through a tenant-qualified Redis backplane.",
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
