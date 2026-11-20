using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Modules;
using OrchardCore.UrlRewriting.Rules;

namespace OrchardCore.UrlRewriting.Services;

public sealed class RewriteOptionsConfiguration : IConfigureOptions<RewriteOptions>
{
    private readonly IRewriteRulesStore _rewriteRulesStore;
    private readonly IServiceProvider _serviceProvider;
    private readonly AdminOptions _adminOptions;
    private readonly ILogger _logger;

    public RewriteOptionsConfiguration(
        IRewriteRulesStore rewriteRulesStore,
        IServiceProvider serviceProvider,
        IOptions<AdminOptions> adminOptions,
        ILogger<RewriteOptionsConfiguration> logger)
    {
        _rewriteRulesStore = rewriteRulesStore;
        _serviceProvider = serviceProvider;
        _adminOptions = adminOptions.Value;
        _logger = logger;
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

            try
            {
                source.Configure(options, rule);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                _logger.LogError(ex, "Unable to configure the URL rewriting rule '{RuleName}'.", rule.Name);
            }
        }

        if (options.Rules.Count > 0)
        {
            // Exclude URIs prefixed with 'admin' to prevent accidental access restrictions caused by the provided rules.
            var prefix = new PathString('/' + _adminOptions.AdminUrlPrefix.TrimStart('/'));

            options.Rules.Insert(0, new ExcludeUrlPrefixRule(prefix));
        }
    }
}
