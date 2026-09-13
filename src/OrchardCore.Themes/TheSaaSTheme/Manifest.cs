using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Modules.Manifest;

[assembly: Theme(
    Name = "The SaaS Theme",
    BaseTheme = "TheTheme",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "A polished site and authentication experience for SaaS tenants.",
    Tags = ["Bootstrap", "SaaS"]
)]
