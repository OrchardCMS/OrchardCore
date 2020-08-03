using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Block Detection",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "This module ouputs a warning to the log when blocking calls are made on the ThreadPool.",
    Category = "Infrastructure"
)]
