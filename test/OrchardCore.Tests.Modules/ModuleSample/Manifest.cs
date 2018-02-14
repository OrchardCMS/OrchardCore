using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Module Sample",
    Author = "Nicholas Mayne",
    Website = "http://orchardproject.net",
    Version = "2.0.0",
    Category = "Test"
)]

[assembly: Feature(
    Id = "Sample1",
    Name = "Sample 1",
    Description = "Feature with no dependencies.",
    Dependencies = ""
)]

[assembly: Feature(
    Id = "Sample2",
    Name = "Sample 2",
    Description = "Module with dependency one sample 1.",
    Dependencies = "Sample1"
)]

[assembly: Feature(
    Id = "Sample3",
    Name = "Sample 3",
    Description = "Module with dependency one sample 2.",
    Dependencies = "Sample2"
)]

[assembly: Feature(
    Id = "Sample4",
    Name = "Sample 4",
    Description = "Module with dependency one sample 2.",
    Dependencies = "Sample2"
)]
