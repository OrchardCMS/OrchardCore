namespace OrchardCore.ContentManagement;

/// <summary>
/// A <see cref="IContent" /> that contains a <see cref="Html" /> property.
/// </summary>
public interface IHtmlHolderContent : IContent
{
    /// <summary>
    /// Gets or sets raw HTML.
    /// </summary>
    string Html { get; set; }
}
