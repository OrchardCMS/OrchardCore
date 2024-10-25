using OrchardCore.Google;
using OrchardCore.Modules.Manifest;

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
    Description = "Authenticates users with their Google Account.",
    Dependencies =
    [
        "OrchardCore.Users.ExternalAuthentication",
    ]
)]

[assembly: Feature(
    Id = GoogleConstants.Features.GoogleAnalytics,
    Name = "Google Analytics",
    Category = "Google",
    Description = "Integrate Google Analytics (gtag.js)"
)]

[assembly: Feature(
    Id = GoogleConstants.Features.GoogleTagManager,
    Name = "Google Tag Manager",
    Category = "Google",
    Description = "Integrate Google Tag Manager"
)]
