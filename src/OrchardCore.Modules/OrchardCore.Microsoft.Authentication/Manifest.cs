using OrchardCore.Modules.Manifest;
using OrchardCore.Microsoft.Authentication;

[assembly: Module(
    Name = "Microsoft.Authentication",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Category = "Authentication"
)]

[assembly: Feature(
    Id = MicrosoftAuthenticationConstants.Features.MSE,
    Name = "Microsoft Account Authentication",
    Category = "Authentication",
    Description = "Authenticates users with their Microsoft Account."
)]

[assembly: Feature(
    Id = MicrosoftAuthenticationConstants.Features.AAD,
    Name = "Microsoft Azure Active Directory Authentication",
    Category = "Authentication",
    Description = "Authenticates users with their Azure Active Directory Account."
)]