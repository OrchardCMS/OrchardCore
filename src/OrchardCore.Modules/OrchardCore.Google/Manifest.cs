using OrchardCore.Modules.Manifest;
using OrchardCore.Google;

[assembly: Module(
    Name = "Google",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "Google"
)]

[assembly: Feature(
    Id = GoogleConstants.Features.GoogleAuthentication,
    Name = "Google Authentication",
    Category = "Google",
    Description = "Authenticates users with their Google Account."
)]

[assembly: Feature(
    Id = GoogleConstants.Features.GoogleAnalytics,
    Name = "Google Analytics",
    Category = "Google",
    Description = "Integrate Google Analytics gtagjs"
)]
