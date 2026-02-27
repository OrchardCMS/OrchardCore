using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OpenID Connect Secrets",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.OpenId.Secrets",
    Name = "OpenID Connect Secrets",
    Description = "Allows storing OpenID Connect signing and encryption keys as secrets.",
    Dependencies =
    [
        "OrchardCore.OpenId.Server",
        "OrchardCore.Secrets",
    ],
    Category = "OpenID Connect"
)]
