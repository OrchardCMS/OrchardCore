using OrchardCore.Data.Documents;

namespace OrchardCore.UrlRewriting.Models;

public sealed class RewriteRulesDocument : Document
{
    public List<RewriteRule> Rules { get; set; } = [];
}
