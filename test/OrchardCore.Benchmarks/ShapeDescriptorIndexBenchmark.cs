using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Benchmark.Support;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Environment.Extensions;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Benchmark;

[MemoryDiagnoser]
public class ShapeDescriptorIndexBenchmark
{
    private readonly ConcurrentDictionary<string, FeatureShapeDescriptor> _shapeDescriptors = new();

    [GlobalSetup]
    public async Task SetupAsync()
    {
        using var content = new BlogContext();
        await content.InitializeAsync();
        await content.UsingTenantScopeAsync(async scope =>
        {
            var bindingStrategies = scope.ServiceProvider.GetRequiredService<IEnumerable<IShapeTableProvider>>();
            var typeFeatureProvider = scope.ServiceProvider.GetRequiredService<ITypeFeatureProvider>();
            foreach (var bindingStrategy in bindingStrategies)
            {
                var strategyFeature = typeFeatureProvider.GetFeatureForDependency(bindingStrategy.GetType());

                var builder = new ShapeTableBuilder(strategyFeature, []);
                await bindingStrategy.DiscoverAsync(builder);
                BuildDescriptors(bindingStrategy, builder.BuildAlterations());
            }
        });
    }

    [Benchmark(Baseline = true)]
    public List<ShapeDescriptorIndex> SingleLoopLists()
    {
        return _shapeDescriptors
            .GroupBy(sd => sd.Value.ShapeType, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ShapeDescriptorIndex
            (
                shapeType: group.Key,
                alterationKeys: group.Select(kv => kv.Key),
                descriptors: _shapeDescriptors
            ))
            .ToList();
    }

    [Benchmark]
    public List<MultiSelectShapeDescriptorIndex> MultipleLoopsUsingLists()
    {
        return _shapeDescriptors
            .GroupBy(sd => sd.Value.ShapeType, StringComparer.OrdinalIgnoreCase)
            .Select(group => new MultiSelectShapeDescriptorIndex
            (
                shapeType: group.Key,
                alterationKeys: group.Select(kv => kv.Key),
                descriptors: _shapeDescriptors
            ))
            .ToList();
    }

    [Benchmark]
    public List<MultiSelectShapeDescriptorIndexArray> MultipleLoopsArrays()
    {
        return _shapeDescriptors
            .GroupBy(sd => sd.Value.ShapeType, StringComparer.OrdinalIgnoreCase)
            .Select(group => new MultiSelectShapeDescriptorIndexArray
            (
                shapeType: group.Key,
                alterationKeys: group.Select(kv => kv.Key),
                descriptors: _shapeDescriptors
            ))
            .ToList();
    }

    private void BuildDescriptors(IShapeTableProvider bindingStrategy, IEnumerable<ShapeAlteration> builtAlterations)
    {
        var alterationSets = builtAlterations.GroupBy(a => a.Feature.Id + a.ShapeType);

        foreach (var alterations in alterationSets)
        {
            var firstAlteration = alterations.First();

            var key = bindingStrategy.GetType().Name
                + firstAlteration.Feature.Id
                + firstAlteration.ShapeType.ToLower();

            if (!_shapeDescriptors.ContainsKey(key))
            {
                var descriptor = new FeatureShapeDescriptor
                (
                    firstAlteration.Feature,
                    firstAlteration.ShapeType
                );

                foreach (var alteration in alterations)
                {
                    alteration.Alter(descriptor);
                }

                _shapeDescriptors[key] = descriptor;
            }
        }
    }
}
