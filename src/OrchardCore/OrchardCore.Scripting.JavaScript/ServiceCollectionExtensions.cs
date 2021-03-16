using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Scripting.JavaScript
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJavaScriptEngine(this IServiceCollection services)
        {
            services.AddSingleton<IScriptingEngine, JavaScriptEngine>();

            return services;
        }
    }
}
