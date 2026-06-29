using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "ClamAV Antivirus Scanner",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Antivirus.ClamAV",
    Name = "ClamAV Antivirus Scanner",
    Description = "Scans files with ClamAV before Orchard Core stores them.",
    Category = "Security"
)]
