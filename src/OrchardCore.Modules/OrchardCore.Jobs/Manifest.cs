using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Jobs",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Runs jobs in the background, once whatever the number of servers, at once or on a schedule, and keeps the history of their runs.",
    Category = "Tasks"
)]
