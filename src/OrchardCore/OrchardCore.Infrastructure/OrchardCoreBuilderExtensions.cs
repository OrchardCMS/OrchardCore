using Microsoft.AspNetCore.Builder;
using OrchardCore;
using OrchardCore.Infrastructure.ErrorHandling;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides an extension method for <see cref="OrchardCoreBuilder"/>.
/// </summary>
public static partial class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Adds the services and the middleware attaching an RFC 9457 Problem Details body to the error
    /// responses meant for headless clients, i.e. the ones that opted out of the HTML error pages.
    /// </summary>
    /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
    public static OrchardCoreBuilder AddApiProblemDetails(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Registers the 'IProblemDetailsService' used by the middleware below to write the
            // bodies, honoring the app-level Problem Details customizations.
            services.AddProblemDetails();
        });

        // Note: the middleware inspects the responses produced by the rest of the pipeline, so it
        // must be registered before the authentication and authorization middlewares, that are the
        // main source of the error responses it is responsible for.
        builder.Configure(app => app.UseMiddleware<ApiProblemDetailsMiddleware>(),
            order: OrchardCoreConstants.ConfigureOrder.Security);

        return builder;
    }

    /// <summary>
    /// Adds phone format validator service.
    /// </summary>
    /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
    public static OrchardCoreBuilder AddPhoneFormatValidator(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddTransient<IPhoneFormatValidator, PhoneFormatValidator>();
        });

        return builder;
    }
}
