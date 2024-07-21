using System.IO;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement;

public record ResourcesTagHelperProcessorContext(
    ResourceTagType Type,
    TextWriter Writer);
