using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "SignalR Azure Backplane",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Routes SignalR messages across application nodes through the Azure SignalR Service.",
    Category = "Infrastructure"
)]

[assembly: Feature(
    Id = "OrchardCore.SignalR.Azure",
    Name = "SignalR Azure Backplane",
    Description = "Uses the Azure SignalR Service as the SignalR backplane, enabling multi-instance deployments.",
    Category = "Infrastructure",
    Dependencies =
    [
        "OrchardCore.SignalR",
    ]
)]
