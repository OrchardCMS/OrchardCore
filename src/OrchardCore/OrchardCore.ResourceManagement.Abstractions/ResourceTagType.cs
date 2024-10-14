namespace OrchardCore.ResourceManagement;

/// <summary>
/// The possible <c>type</c> values of the <c>&lt;resources type="..." /&gt;</c> Razor tag helper and the
/// <c>{% resources type: "..." %}</c> liquid tag, indicating the kinds of resources to be rendered. The value should be
/// chosen based on the tag's location in the document.
/// </summary>
public enum ResourceTagType
{
    /// <summary>
    /// Resources that should be rendered along with <see cref="IResourceManager.RenderMeta"/>.
    /// </summary>
    Meta,

    /// <summary>
    /// Resources that should be rendered along with <see cref="IResourceManager.RenderHeadLink"/>.
    /// </summary>
    HeadLink,

    /// <summary>
    /// Resources that should be rendered along with <see cref="IResourceManager.RenderStylesheet"/>.
    /// </summary>
    Stylesheet,

    /// <summary>
    /// Resources that should be rendered along with <see cref="IResourceManager.RenderHeadScript"/>.
    /// </summary>
    HeadScript,

    /// <summary>
    /// Resources that should be rendered along with <see cref="IResourceManager.RenderFootScript"/>.
    /// </summary>
    FootScript,

    /// <summary>
    /// Resources that should be rendered inside the <c>/html/head</c> element.
    /// </summary>
    Header,

    /// <summary>
    /// Resources that should be rendered inside the <c>/html/head</c> element, near the end of the document.
    /// </summary>
    Footer
}
