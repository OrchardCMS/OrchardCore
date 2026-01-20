using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.UrlRewriting.Rules;

namespace OrchardCore.UrlRewriting.Services;

public sealed class RewriteOptionsConfiguration : IConfigureOptions<RewriteOptions>
{
    private readonly IRewriteRulesStore _rewriteRulesStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly AdminOptions _adminOptions;

    public RewriteOptionsConfiguration(
        IRewriteRulesStore rewriteRulesStore,
        IServiceProvider serviceProvider,
        IOptions<AdminOptions> adminOptions)
    {
        _rewriteRulesStore = rewriteRulesStore;
        _serviceProvider = serviceProvider;
        _adminOptions = adminOptions.Value;
    }

    public void Configure(RewriteOptions options)
    {
        var rules = _rewriteRulesStore.GetAllAsync()
            .GetAwaiter()
            .GetResult();

        foreach (var rule in rules.OrderBy(r => r.Order).ThenBy(r => r.CreatedUtc))
        {
            var source = _serviceProvider.GetKeyedService<IUrlRewriteRuleSource>(rule.Source);

            if (source == null)
            {
                continue;
            }

            source.Configure(options, rule);
        }

        if (options.Rules.Count > 0)
        {
            // Exclude URIs prefixed with 'admin' to prevent accidental access restrictions caused by the provided rules.
            var prefix = new PathString('/' + _adminOptions.AdminUrlPrefix.TrimStart('/'));

            options.Rules.Insert(0, new ExcludeUrlPrefixRule(prefix));
        }
    }
}
