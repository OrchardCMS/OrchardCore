using OrchardCore;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides an extension method for <see cref="OrchardCoreBuilder"/>.
/// </summary>
public static partial class OrchardCoreBuilderExtensions
{
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
