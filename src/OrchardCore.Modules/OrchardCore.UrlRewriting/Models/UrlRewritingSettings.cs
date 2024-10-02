namespace OrchardCore.UrlRewriting.Models;

public sealed class UrlRewritingSettings
{
    public string ApacheModRewrite { get; set; }

    public List<RewriteRule> Rules { get; set; }
}
