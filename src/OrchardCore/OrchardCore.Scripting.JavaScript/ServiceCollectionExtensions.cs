using System.Text.Json.Dynamic;
using System.Text.Json.Nodes;
using Jint;
using Jint.Runtime.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Scripting.JavaScript;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJavaScriptEngine(this IServiceCollection services)
    {
        services.AddSingleton<IScriptingEngine, JavaScriptEngine>();

        services.Configure<Options>(option => option.SetWrapObjectHandler(static (e, target, type) =>
        {
            return target switch
            {
                JsonDynamicObject dynamicObject => ObjectWrapper.Create(e, (JsonObject)dynamicObject, type),
                JsonDynamicArray dynamicArray => ObjectWrapper.Create(e, (JsonArray)dynamicArray, type),
                JsonDynamicValue dynamicValue => ObjectWrapper.Create(e, (JsonValue)dynamicValue, type),
                StringValues stringValues => ObjectWrapper.Create(e, stringValues.Count <= 1 ? stringValues.ToString() : stringValues.ToArray(), type),
                _ => ObjectWrapper.Create(e, target, type)
            };
        }));

        return services;
    }
}
