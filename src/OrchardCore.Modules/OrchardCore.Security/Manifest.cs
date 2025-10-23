using OrchardCore.Modules.Manifest;
using OrchardCore.Security.Core;

[assembly: Module(
    Name = "Security",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = SecurityConstants.Features.Area,
    Name = "Security",
    Description = "The Security module adds HTTP headers to follow security best practices.",
    Category = "Security"
)]

[assembly: Feature(
    Id = SecurityConstants.Features.Credentials,
    Name = "Security Credentials",
    Description = "Provides a way to securly manage reusable credentials.",
    Category = "Security",
    EnabledByDependencyOnly = true
)]
