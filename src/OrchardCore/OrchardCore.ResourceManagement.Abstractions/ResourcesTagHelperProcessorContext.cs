namespace OrchardCore.ResourceManagement;

/// <summary>
/// The context passed to <see cref="IResourcesTagHelperProcessor.ProcessAsync"/>, to render the
/// <c>&lt;resources /&gt;</c> Razor tag helper and the <c>{% resources %}</c> liquid tag.
/// </summary>
/// <param name="Type">The value indicating which types of resources to render.</param>
/// <param name="Writer">The object that writes the rendered content into the HTML output.</param>
public record ResourcesTagHelperProcessorContext(
    ResourceTagType Type,
    TextWriter Writer);
