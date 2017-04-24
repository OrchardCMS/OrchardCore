using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.Tokens.Services;

namespace Orchard.Tokens
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a fallback tokenizer in case the Tokenizer module is not enabled.
        /// </summary>
        public static IServiceCollection AddNullTokenizer(this IServiceCollection services)
        {
            services.TryAddSingleton<ITokenizer, NullTokenizer>();

            return services;
        }
    }
}
