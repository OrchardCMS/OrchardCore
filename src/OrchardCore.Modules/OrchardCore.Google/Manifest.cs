using OrchardCore.Modules.Manifest;
using OrchardCore.Google;

[assembly: Module(
    Name = "Google",
    Author = "The Orchard Team",
    Website = "https://orchardproject.net",
    Version = "2.0.0",
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
