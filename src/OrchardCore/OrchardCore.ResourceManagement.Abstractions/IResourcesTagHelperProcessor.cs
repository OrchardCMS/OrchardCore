namespace OrchardCore.ResourceManagement;

/// <summary>
/// Processes resources in the <c>&lt;resources /&gt;</c> tag helper.
/// </summary>
public interface IResourcesTagHelperProcessor
{
    /// <summary>
    /// Invoked when rendering registered resources.
    /// </summary>
    Task ProcessAsync(ResourcesTagHelperProcessorContext context);
}
