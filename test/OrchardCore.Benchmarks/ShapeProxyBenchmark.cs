using System;
using System.Collections.Concurrent;
using BenchmarkDotNet.Attributes;
using Castle.DynamicProxy;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;

namespace OrchardCore.Benchmark
{
    /*
    | Method                       | Mean       | Error      | StdDev    | Gen0   | Allocated |
    |----------------------------- |-----------:|-----------:|----------:|-------:|----------:|
    | CreateInstance               |   9.774 ns |  17.795 ns | 0.9754 ns | 0.0093 |      88 B |
    | CreateDynamicProxy           | 722.913 ns | 121.855 ns | 6.6793 ns | 0.2222 |    2096 B |
    | CreateCachedProxy            | 233.722 ns |  65.978 ns | 3.6165 ns | 0.0756 |     712 B |
    */

    [MemoryDiagnoser]
    [ShortRunJob]
    public class ShapeProxyBenchmark
    {
        private static ConcurrentDictionary<Type, Type> _proxyTypesCache = [];
        private static readonly ProxyGenerator _proxyGenerator = new();
        private static readonly Type _proxyType;

        static ShapeProxyBenchmark()
        {
            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(new ShapeViewModel());
            _proxyType = _proxyGenerator.CreateClassProxy(typeof(MenuItem), options).GetType();
        }

        [Benchmark]
        public static object CreateInstance()
        {
            var shape = (IShape)Activator.CreateInstance(typeof(ContentItemViewModel));
            return shape;
        }

        [Benchmark]
        public static object CreateDynamicProxy()
        {
            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(new ShapeViewModel());
            return (IShape)_proxyGenerator.CreateClassProxy(typeof(MenuItem), options);
        }

        [Benchmark]
        public static object CreateCachedProxy()
        {
            if (_proxyTypesCache.TryGetValue(typeof(MenuItem), out var proxyType))
            {
                var model = new ShapeViewModel();
                return (IShape)Activator.CreateInstance(_proxyType, model, model, Array.Empty<IInterceptor>());
            }

            var options = new ProxyGenerationOptions();
            options.AddMixinInstance(new ShapeViewModel());
            var shape = (IShape)_proxyGenerator.CreateClassProxy(typeof(MenuItem), options);

            _proxyTypesCache.TryAdd(typeof(MenuItem), shape.GetType());

            return shape;
        }
    }
}
