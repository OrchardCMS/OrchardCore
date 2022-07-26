using OrchardCore.Modules.Manifest;
using OrchardCore.Twitter;

[assembly: Module(
    Name = "Twitter",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "Twitter"
)]

[assembly: Feature(
    Id = TwitterConstants.Features.TwitterAuthentication,
    Name = "Twitter Authentication",
    Category = "Twitter",
    Description = "Authenticates users with their Twitter Account."
)]
