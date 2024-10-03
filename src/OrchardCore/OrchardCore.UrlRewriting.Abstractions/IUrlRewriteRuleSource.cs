using Microsoft.AspNetCore.Rewrite;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting;

public interface IUrlRewriteRuleSource
{
    string Name { get; }

    void Configure(RewriteOptions options, RewriteRule rule);
}
