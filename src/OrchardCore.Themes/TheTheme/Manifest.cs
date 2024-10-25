using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Modules.Manifest;

[assembly: Theme(
    Name = "The Default Theme",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The default Theme.",
    Dependencies =
    [
        "OrchardCore.Themes",
    ],
    Tags =
    [
        "Bootstrap",
        "Default",
    ]
)]
