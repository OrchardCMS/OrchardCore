namespace Microsoft.AspNetCore.Builder;

public static class PoweredByOrchardCoreExtensions
{
    /// <summary>
    /// Configures whether use or not the Header X-Powered-By.
    /// Default value is OrchardCore.
    /// </summary>
    /// <param name="app">The modular application builder.</param>
    /// <param name="enabled">Boolean indicating if the header should be included in the response or not.</param>
    /// <returns>The modular application builder.</returns>
    public static IApplicationBuilder UsePoweredByOrchardCore(this IApplicationBuilder app, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(app);

        return enabled
            ? app.UsePoweredBy()
            : app;
    }

    /// <summary>
    /// Configures whether use or not the Header X-Powered-By and its value.
    /// Default value is OrchardCore.
    /// </summary>
    /// <param name="app">The modular application builder.</param>
    /// <param name="enabled">Boolean indicating if the header should be included in the response or not.</param>
    /// <param name="headerValue">Header's value.</param>
    /// <returns>The modular application builder.</returns>
    public static IApplicationBuilder UsePoweredBy(this IApplicationBuilder app, bool enabled, string headerValue)
    {
        return enabled
            ? app.UsePoweredBy(options => options.HeaderValue = headerValue)
            : app;
    }
}
