using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;

namespace OrchardCore.Benchmarks.Support;

/// <summary>
/// Gets the runtime path of <c>ShapeFactoryExtensions.CreateAsync&lt;TModel&gt;()</c> as a delegate: a delegate
/// invocation is not intercepted by the shape source generator, so it creates a Castle DynamicProxy proxy of the
/// model, as the call sites the generator doesn't intercept do. The method is resolved by reflection since another
/// <c>ShapeFactoryExtensions</c> type exists in the navigation assembly, in the same namespace.
/// </summary>
internal static class ShapeFactoryRuntimePath
{
    public static Func<IShapeFactory, string, Action<TModel>, ValueTask<IShape>> GetCreateAsync<TModel>() where TModel : class
    {
        var extensions = typeof(DefaultShapeFactory).Assembly.GetType("OrchardCore.DisplayManagement.ShapeFactoryExtensions");

        var method = extensions
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == "CreateAsync" && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 &&
                m.GetParameters() is [{ ParameterType.Name: nameof(IShapeFactory) }, { ParameterType: var shapeType }, { ParameterType: var initialize }] &&
                shapeType == typeof(string) && initialize.IsGenericType && initialize.GetGenericTypeDefinition() == typeof(Action<>));

        return method.MakeGenericMethod(typeof(TModel)).CreateDelegate<Func<IShapeFactory, string, Action<TModel>, ValueTask<IShape>>>();
    }
}
