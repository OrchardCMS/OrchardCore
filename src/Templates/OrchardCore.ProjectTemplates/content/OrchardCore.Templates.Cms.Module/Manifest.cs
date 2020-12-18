using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "OrchardCore.Templates.Cms.Module",
    Author = "The Orchard Core Team",
    Website = "https://orchardcore.net",
    Version = "0.0.1",
    Description = "OrchardCore.Templates.Cms.Module",
#if (AddPart)
    Dependencies = new[] { "OrchardCore.Contents" },
#endif
    Category = "Content Management"
)]
