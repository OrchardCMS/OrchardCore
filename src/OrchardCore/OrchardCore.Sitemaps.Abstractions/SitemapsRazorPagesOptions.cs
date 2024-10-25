namespace OrchardCore.Sitemaps;

public class SitemapsRazorPagesOptions
{

    private readonly List<SitemapsRazorPagesContentTypeOption> _contentTypeOptions = [];

    public SitemapsRazorPagesOptions ConfigureContentType(string contentType, Action<SitemapsRazorPagesContentTypeOption> action)
    {
        var option = new SitemapsRazorPagesContentTypeOption(contentType);

        action(option);

        _contentTypeOptions.Add(option);

        return this;
    }

    public IReadOnlyList<SitemapsRazorPagesContentTypeOption> ContentTypeOptions => _contentTypeOptions;
}
