using OrchardCore.DisplayManagement;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting.ViewModels;

public class RewriteRuleEntry
{
    public RewriteRule Rule { get; set; }

    public IShape Shape { get; set; }
}
