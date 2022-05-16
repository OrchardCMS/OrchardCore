using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "ReCaptchaV3",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion)]

[assembly: Feature(
    Id = "OrchardCore.ReCaptchaV3",
    Name = "ReCaptchaV3",
    Category = "Security",
    Description = "Provides core ReCaptchaV3 functionality.")]
