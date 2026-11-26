using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Sitemaps.Builders;

public class SitemapBuilderContext
{
    private string _hostPrefix;

    /// <summary>
    /// Gets or sets the scheme and host the sitemap URLs start with, without a trailing slash.
    /// Builders append root-relative paths to it, so a trailing slash, as in
    /// <c>ISite.BaseUrl</c>, is removed.
    /// </summary>
    public string HostPrefix
    {
        get => _hostPrefix;
        set => _hostPrefix = value?.TrimEnd('/');
    }

    public IUrlHelper UrlHelper { get; set; }
    public SitemapResponse Response { get; set; }
}

public class SitemapResponse
{
    public XElement ResponseElement { get; set; }
}
