using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Modules.Manifest;

[assembly: Theme(
    Name = "The Admin Theme",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "The default Admin theme.",
    Tags = new[] { ManifestConstants.AdminTag }
)]
