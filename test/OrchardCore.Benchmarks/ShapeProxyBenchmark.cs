using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using Castle.DynamicProxy;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;

namespace OrchardCore.Benchmark;

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
    private static readonly ConcurrentDictionary<Type, Type> _proxyTypesCache = [];
    private static readonly ProxyGenerator _proxyGenerator = new();
    private static readonly Type _proxyType;

    static ShapeProxyBenchmark()
    {
        var options = new ProxyGenerationOptions();
        options.AddMixinInstance(new ShapeViewModel());
        _proxyType = _proxyGenerator.CreateClassProxy<MenuItem>(options).GetType();
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
        return (IShape)_proxyGenerator.CreateClassProxy<MenuItem>(options);
    }

    [Benchmark]
    public object CreateCachedProxy()
    {
        if (_proxyTypesCache.TryGetValue(typeof(MenuItem), out var _))
        {
            var model = new ShapeViewModel();
            return (IShape)Activator.CreateInstance(_proxyType, model, model, Array.Empty<IInterceptor>());
        }

        var options = new ProxyGenerationOptions();
        options.AddMixinInstance(new ShapeViewModel());
        var shape = (IShape)_proxyGenerator.CreateClassProxy<MenuItem>(options);

        _proxyTypesCache.TryAdd(typeof(MenuItem), shape.GetType());

        return shape;
    }
}
