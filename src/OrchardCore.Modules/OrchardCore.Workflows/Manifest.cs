using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Workflows",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The Workflows module provides tools and APIs to create custom workflows",
    Category = "Workflows"
)]

[assembly: Feature(
    Id = "OrchardCore.Workflows",
    Name = "Workflows",
    Description = "The Workflows module provides tools and APIs to create custom workflows",
    Dependencies = new[] { "OrchardCore.Liquid", "OrchardCore.Scripting" },
    Category = "Workflows"
)]

[assembly: Feature(
    Id = "OrchardCore.Workflows.Http",
    Name = "HTTP Workflows Activities",
    Description = "Provides HTTP-related services and activities.",
    Dependencies = new[] { "OrchardCore.Workflows" },
    Category = "Workflows"
)]

[assembly: Feature(
    Id = "OrchardCore.Workflows.Timers",
    Name = "Timer Workflows Activities",
    Description = "Provides timer-based services and activities.",
    Dependencies = new[] { "OrchardCore.Workflows" },
    Category = "Workflows"
)]

[assembly: Feature(
    Id = "OrchardCore.Workflows.Session",
    Name = "Session Workflows Activities",
    Description = "Provides 'YesSql' Session-related activities.",
    Dependencies = new[] { "OrchardCore.Workflows" },
    Category = "Workflows"
)]
