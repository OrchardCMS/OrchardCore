using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Modules.Manifest;

[assembly: Theme(
    Name = "Admin Theme Sample",
    Description = "A minimal admin theme without a base theme or custom assets.",
    Dependencies = ["OrchardCore.Admin"],
    Tags = [ManifestConstants.AdminTag]
)]
