using Microsoft.Extensions.DependencyInjection;
using Orchard.Tokens.Services;

namespace Orchard.Tokens
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Tokens services.
        /// </summary>
        public static IServiceCollection AddTokensServices(this IServiceCollection services)
        {
            services.AddScoped<ITokenizer, Tokenizer>();
            services.AddSingleton<TokensHelper>();
            return services;
        }
    }
}