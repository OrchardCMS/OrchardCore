using OrchardCore.Data.Documents;

namespace OrchardCore.UrlRewriting.Models;

public sealed class RewriteRulesDocument : Document
{
    public Dictionary<string, RewriteRule> Rules { get; set; } = [];
}
