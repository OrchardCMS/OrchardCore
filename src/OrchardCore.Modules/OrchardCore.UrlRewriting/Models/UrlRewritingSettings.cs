namespace OrchardCore.UrlRewriting.Models;

public class UrlRewritingSettings
{
    public string ApacheModRewrite { get; set; }

    public List<Rule> Rules { get; set; }
}

public class Rule
{
    public string Name { get; set; }

    public string Pattern { get; set; }

    public string Substitution { get; set; }

    public string Flags { get; set; }
}
