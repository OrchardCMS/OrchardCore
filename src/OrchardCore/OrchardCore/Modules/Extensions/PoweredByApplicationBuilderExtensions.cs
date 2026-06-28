using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace Microsoft.AspNetCore.Builder;

public static class PoweredByApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePoweredBy(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<PoweredByMiddleware>();
    }

    public static IApplicationBuilder UsePoweredBy(this IApplicationBuilder app, Action<PoweredByOptions> optionsAction)
    {
        ArgumentNullException.ThrowIfNull(app);

        ArgumentNullException.ThrowIfNull(optionsAction);

        var options = new PoweredByOptions();

        optionsAction.Invoke(options);

        return app.UseMiddleware<PoweredByMiddleware>(Options.Create(options));
    }

}
