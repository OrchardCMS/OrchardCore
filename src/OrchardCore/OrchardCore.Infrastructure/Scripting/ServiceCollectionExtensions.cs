using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting.Files;

namespace OrchardCore.Scripting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScripting(this IServiceCollection services)
        {
            services.AddSingleton<IScriptingManager, DefaultScriptingManager>();
            services.AddSingleton<IGlobalMethodProvider, CommonGeneratorMethods>();

            services.AddSingleton<IScriptingEngine, FilesScriptEngine>();
            return services;
        }
    }
}
