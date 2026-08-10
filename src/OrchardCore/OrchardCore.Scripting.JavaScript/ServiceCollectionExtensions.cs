using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;
using Jint;
using Jint.Runtime.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using JintOptions = Jint.Options;

namespace OrchardCore.Scripting.JavaScript;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJavaScriptEngine(this IServiceCollection services)
    {
        services.AddSingleton<IScriptingEngine, JavaScriptEngine>();

        // Registered before any configuration of Jint's options an application adds, so that the default
        // execution constraints it applies can still be changed or removed by the application.
        services.AddTransient<IConfigureOptions<JintOptions>, JintOptionsConfiguration>();

        services.Configure<JintOptions>(option =>
        {
            option.ExperimentalFeatures |= ExperimentalFeature.TaskInterop;

            option.SetWrapObjectHandler(static (e, target, type) => target switch
            {
                JsonDynamicObject dynamicObject => ObjectWrapper.Create(e, (JsonObject)dynamicObject, type),
                JsonDynamicArray dynamicArray => ObjectWrapper.Create(e, (JsonArray)dynamicArray, type),
                JsonDynamicValue dynamicValue => ObjectWrapper.Create(e, (JsonValue)dynamicValue, type),
                StringValues stringValues => ObjectWrapper.Create(e, stringValues.Count <= 1 ? stringValues.ToString() : stringValues.ToArray(), type),
                _ => ObjectWrapper.Create(e, target, type)
            });
        });

        return services;
    }
}
