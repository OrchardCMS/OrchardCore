using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Remote Deployment",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Provide the ability to export and import to and from a remote server.",
    Dependencies = new[] { "OrchardCore.Deployment" },
    Category = "Deployment"
)]
