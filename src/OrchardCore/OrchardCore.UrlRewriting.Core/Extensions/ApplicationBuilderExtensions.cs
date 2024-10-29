using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.UrlRewriting.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseUrlRewriting(this IApplicationBuilder builder, IServiceProvider serviceProvider)
    {
        var rewriteOptions = serviceProvider.GetRequiredService<IOptions<RewriteOptions>>().Value;

        builder.UseRewriter(rewriteOptions);

        return builder;
    }
}
