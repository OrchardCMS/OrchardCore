namespace OrchardCore.ContentManagement;

public interface IHtmlHolderContent : IContent
{
    /// <summary>
    /// Gets or sets raw HTML.
    /// </summary>
    public string Html { get; set; }
}
