using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Benchmarks.Support;
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
| Method                           | Mean        | Error        | StdDev     | Gen0   | Allocated |
|--------------------------------- |------------:|-------------:|-----------:|-------:|----------:|
| CreateInstance                   |    21.88 ns |     4.905 ns |   0.269 ns | 0.0006 |      88 B |
| CreateDynamicProxy               | 1,572.21 ns | 3,016.084 ns | 165.322 ns | 0.0134 |    2048 B |
| CreateCachedProxy                |   484.60 ns |   261.948 ns |  14.358 ns | 0.0048 |     728 B |
| CreateDynamicShapeThroughFactory |   174.04 ns |    66.406 ns |   3.640 ns | 0.0041 |     576 B |
| CreateProxyThroughFactory        |   317.52 ns |   278.306 ns |  15.255 ns | 0.0067 |     936 B |

The proxy created through the factory, with its compiled constructor call, costs the difference between the two
factory rows: about 145 ns and 360 B, against 485 ns and 728 B for the cached proxy type instantiated with
'Activator.CreateInstance' and its arguments.
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

    // A delegate invocation is not intercepted by the shape source generator, so this calls the runtime path of
    // the factory extension, which creates a proxy of the model with its cached compiled factory.
    private static readonly Func<IShapeFactory, string, Action<MenuItem>, ValueTask<IShape>> s_createProxyShape = ShapeFactoryRuntimePath.GetCreateAsync<MenuItem>();

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
