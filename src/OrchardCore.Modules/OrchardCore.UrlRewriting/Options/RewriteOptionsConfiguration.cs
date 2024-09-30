using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Documents;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Rules;

namespace OrchardCore.UrlRewriting.Options;

public sealed class RewriteOptionsConfiguration : IConfigureOptions<RewriteOptions>
{
    private readonly IDocumentManager<RewriteRulesDocument> _documentManager;

    private readonly AdminOptions _adminOptions;

    public RewriteOptionsConfiguration(IDocumentManager<RewriteRulesDocument> documentManager, IOptions<AdminOptions> adminOptions)
    {
        _documentManager = documentManager;
        _adminOptions = adminOptions.Value;
    }

    public void Configure(RewriteOptions options)
    {
        var rules = _documentManager.GetOrCreateMutableAsync()
            .GetAwaiter()
            .GetResult();

        var apacheRules = ApacheRules.FromModels(rules.Rules);

        using var apacheModRewrite = new StringReader(apacheRules);

        options.AddApacheModRewrite(apacheModRewrite);

        if (options.Rules.Count > 0)
        {
            // Exclude admin ui requests to prevent accidental access bricking by provided rules
            options.Rules.Insert(0, new ExcludeAdminUIRule(_adminOptions.AdminUrlPrefix));
        }
    }
}
