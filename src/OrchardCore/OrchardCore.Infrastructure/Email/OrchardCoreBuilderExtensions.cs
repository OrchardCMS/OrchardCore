using OrchardCore.Email;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides an extension method for <see cref="OrchardCoreBuilder"/>.
    /// </summary>
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds e-mail address validator service.
        /// </summary>
        /// <param name="builder">The <see cref="OrchardCoreBuilder"/>.</param>
        public static OrchardCoreBuilder AddEmailAddressValidator(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddTransient<IEmailAddressValidator, EmailAddressValidator>();
            });

            return builder;
        }
    }
}
