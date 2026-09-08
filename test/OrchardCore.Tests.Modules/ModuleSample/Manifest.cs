using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Module Sample",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Category = "Test"
)]

[assembly: Feature(
    Id = "Sample1",
    Name = "Sample 1",
    Description = "Feature with no dependencies.",
    Dependencies = new string[0]
)]

[assembly: Feature(
    Id = "Sample2",
    Name = "Sample 2",
    Description = "Module with dependency on sample 1.",
    Dependencies = ["Sample1"]
)]

[assembly: Feature(
    Id = "Sample3",
    Name = "Sample 3",
    Description = "Module with dependency on sample 2 and ordered before sample 4.",
    Dependencies = ["Sample2"],
    Before = ["Sample4"]
)]

[assembly: Feature(
    Id = "Sample4",
    Name = "Sample 4",
    Description = "Module with dependency on sample 2.",
    Dependencies = ["Sample2"]
)]
