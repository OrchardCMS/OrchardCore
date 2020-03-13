using OrchardCore.Email;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds email address validator service.
        /// </summary>
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
