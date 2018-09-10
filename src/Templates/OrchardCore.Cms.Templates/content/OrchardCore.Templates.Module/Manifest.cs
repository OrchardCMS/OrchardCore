using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrchardCore.Templates.Module",
    Author = "The Orchard Team",
    Website = "http://orchardproject.net",
    Version = "0.0.1",
    Description = "OrchardCore.Templates.Module",
#if (AddPart)
    Dependencies = new[] { "OrchardCore.Contents" },
#endif
    Category = "Content Management"
)]
