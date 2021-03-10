using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Mini Profiler",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.MiniProfiler",
    Name = "Mini Profiler",
    Category = "Performance",
    Description = "Adds the Mini Profiler widget to front endpages."
)]

[assembly: Feature(
    Id = "OrchardCore.MiniProfiler.Admin",
    Name = "Mini Profiler Admin",
    Category = "Performance",
    Description = "Adds the Mini Profiler widget to both admin and front end pages.",
    Dependencies = new[] { "OrchardCore.MiniProfiler" }
)]
