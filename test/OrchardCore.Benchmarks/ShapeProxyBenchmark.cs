using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Modules.Manifest;
using OrchardCore.Navigation;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Benchmarks;

/*
| Method                       | Mean       | Error      | StdDev    | Gen0   | Allocated |
|----------------------------- |-----------:|-----------:|----------:|-------:|----------:|
| CreateInstance               |   9.774 ns |  17.795 ns | 0.9754 ns | 0.0093 |      88 B |
| CreateDynamicProxy           | 722.913 ns | 121.855 ns | 6.6793 ns | 0.2222 |    2096 B |
| CreateCachedProxy            | 233.722 ns |  65.978 ns | 3.6165 ns | 0.0756 |     712 B |
*/

[MemoryDiagnoser]
[ShortRunJob]
[SuppressMessage(
    "Performance",
    "CA1822:Mark members as static",
    Justification = "BenchmarkDotNet needs all benchmark methods to be instance-level.")]
public class ShapeProxyBenchmark
{
    private static readonly ConcurrentDictionary<Type, Type> s_proxyTypesCache = [];
    private static readonly ProxyGenerator s_proxyGenerator = new();
    private static readonly Type s_proxyType;

    // A method group converted to a delegate is not intercepted by the shape source generator, so this calls the
    // runtime path of the factory extension, which creates a proxy of the model with its cached compiled factory.
    private static readonly Func<IShapeFactory, string, Action<MenuItem>, ValueTask<IShape>> s_createProxyShape = ShapeFactoryExtensions.CreateAsync;

    private IShapeFactory _factory;

    static ShapeProxyBenchmark()
    {
        var options = new ProxyGenerationOptions();
        options.AddMixinInstance(new ShapeViewModel());
        s_proxyType = s_proxyGenerator.CreateClassProxy<MenuItem>(options).GetType();
    }

    [GlobalSetup]
    public void Setup()
    {
        var shapeTable = new ShapeTable(new Dictionary<string, ShapeDescriptor>(), new Dictionary<string, ShapeBinding>());

        _factory = new DefaultShapeFactory(
            events: [],
            shapeTableManager: new TestShapeTableManager(shapeTable),
            themeManager: new MockThemeManager(new ExtensionInfo("path", new ManifestInfo(new ModuleAttribute()), (x, y) => [])),
            serviceProvider: new ServiceCollection().BuildServiceProvider());
    }

    [Benchmark]
    public object CreateInstance()
    {
        var shape = Activator.CreateInstance<ContentItemViewModel>();
        return shape;
    }

    [Benchmark]
    public object CreateDynamicProxy()
    {
        var options = new ProxyGenerationOptions();
        options.AddMixinInstance(new ShapeViewModel());
        return (IShape)s_proxyGenerator.CreateClassProxy<MenuItem>(options);
    }

    // The approach of ShapeFactoryExtensions before the constructor of the proxy type was compiled into a factory:
    // 'Activator.CreateInstance' resolves the constructor from the arguments on each creation.
    [Benchmark]
    public object CreateCachedProxy()
    {
        if (s_proxyTypesCache.TryGetValue(typeof(MenuItem), out var _))
        {
            var model = new ShapeViewModel();
            return (IShape)Activator.CreateInstance(s_proxyType, model, model, Array.Empty<IInterceptor>());
        }

        var options = new ProxyGenerationOptions();
        options.AddMixinInstance(new ShapeViewModel());
        var shape = (IShape)s_proxyGenerator.CreateClassProxy<MenuItem>(options);

        s_proxyTypesCache.TryAdd(typeof(MenuItem), shape.GetType());

        return shape;
    }

    // The factory alone: the events and contexts of 'DefaultShapeFactory.CreateAsync()' with a dynamic shape.
    [Benchmark]
    public ValueTask<IShape> CreateDynamicShapeThroughFactory()
    {
        return _factory.CreateAsync("MenuItem");
    }

    // The factory and the proxy, through the runtime path of 'ShapeFactoryExtensions.CreateAsync<TModel>()'.
    [Benchmark]
    public ValueTask<IShape> CreateProxyThroughFactory()
    {
        return s_createProxyShape(_factory, "MenuItem", static m => m.Href = "/");
    }
}
