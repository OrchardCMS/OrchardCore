using OrchardCore.Modules.Manifest;
using OrchardCore.Microsoft.Authentication;

[assembly: Module(
    Name = "Microsoft Authentication",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Category = "Microsoft Authentication"
)]

[assembly: Feature(
    Id = MicrosoftAuthenticationConstants.Features.MicrosoftAccount,
    Name = "Microsoft Account Authentication",
    Category = "Microsoft Authentication",
    Description = "Authenticates users with their Microsoft Account."
)]

[assembly: Feature(
    Id = MicrosoftAuthenticationConstants.Features.AAD,
    Name = "Microsoft Azure Active Directory Authentication",
    Category = "Microsoft Authentication",
    Description = "Authenticates users with their Azure Active Directory Account."
)]
