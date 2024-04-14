using OrchardCore.Microsoft.Authentication;
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Microsoft Authentication",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
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
    Name = "Microsoft Entra ID (Azure Active Directory) Authentication",
    Category = "Microsoft Authentication",
    Description = "Authenticates users with their Microsoft Entra ID Account."
)]
