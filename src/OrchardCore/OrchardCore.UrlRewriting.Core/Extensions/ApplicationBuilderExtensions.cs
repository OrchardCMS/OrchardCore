using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.UrlRewriting.Rules;

namespace OrchardCore.UrlRewriting.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseUrlRewriting(this IApplicationBuilder builder, IServiceProvider serviceProvider)
    {
        var rewriteOptions = serviceProvider.GetRequiredService<IOptionsMonitor<RewriteOptions>>();

        // The rewrite middleware copies its rules when it is built, so it is given a single rule
        // that applies the current rules, which are updated without releasing the tenant.
        var options = new RewriteOptions();
        options.Rules.Add(new MonitoredRewriteRule(rewriteOptions));

        builder.UseRewriter(options);

        return builder;
    }
}
