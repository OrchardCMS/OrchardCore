using System.IO;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement;

public record ResourcesTagHelperProcessorContext(
    TextWriter Writer,
    ResourceTagType Type);
