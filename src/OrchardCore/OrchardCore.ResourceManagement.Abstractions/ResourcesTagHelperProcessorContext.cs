using System.Security.AccessControl;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement;

public record ResourcesTagHelperProcessorContext(
    TagHelperContext TagHelperContext,
    TagHelperOutput Output,
    ResourceType Type);
