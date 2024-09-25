namespace OrchardCore.UrlRewriting.Models;

public class UrlRewritingSettings
{
    public string ApacheModRewrite { get; set; }

    public List<RewriteRule> Rules { get; set; }
}
