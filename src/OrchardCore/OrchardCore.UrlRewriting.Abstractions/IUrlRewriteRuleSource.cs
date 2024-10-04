using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Localization;
using OrchardCore.UrlRewriting.Models;

namespace OrchardCore.UrlRewriting;

public interface IUrlRewriteRuleSource
{
    string Name { get; }

    LocalizedString Description { get; }

    void Configure(RewriteOptions options, RewriteRule rule);
}
