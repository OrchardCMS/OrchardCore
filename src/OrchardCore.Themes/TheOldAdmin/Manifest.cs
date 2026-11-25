using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Modules.Manifest;

[assembly: Theme(
    Name = "The Old Admin Theme",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The previous look of The Admin Theme, before its redesign, for sites that need to keep it.",
    Dependencies =
    [
        "OrchardCore.Themes",
    ],
    Tags =
    [
        ManifestConstants.AdminTag,
    ]
)]
