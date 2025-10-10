using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Benchmarks.Support;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Environment.Extensions;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Benchmarks;

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
                alternations: group.Select(kv => kv.Value)
            ))
            .ToList();
    }

    [Benchmark]
    public List<OriginalShapeDescriptorIndex> OriginalSingleLoopLists()
    {
        return _shapeDescriptors
            .GroupBy(sd => sd.Value.ShapeType, StringComparer.OrdinalIgnoreCase)
            .Select(group => new OriginalShapeDescriptorIndex
            (
                shapeType: group.Key,
                alterationKeys: group.Select(kv => kv.Key),
                _shapeDescriptors
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

    public class OriginalShapeDescriptorIndex : OriginalShapeDescriptor
    {
        private readonly List<FeatureShapeDescriptor> _alternationDescriptors = [];
        private readonly List<string> _wrappers = [];
        private readonly List<string> _bindingSources = [];
        private readonly Dictionary<string, ShapeBinding> _bindings = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<Func<ShapeCreatingContext, Task>> _creatingAsync = [];
        private readonly List<Func<ShapeCreatedContext, Task>> _createdAsync = [];
        private readonly List<Func<ShapeDisplayContext, Task>> _displayingAsync = [];
        private readonly List<Func<ShapeDisplayContext, Task>> _processingAsync = [];
        private readonly List<Func<ShapeDisplayContext, Task>> _displayedAsync = [];

        public OriginalShapeDescriptorIndex(
            string shapeType,
            IEnumerable<string> alterationKeys,
            ConcurrentDictionary<string, FeatureShapeDescriptor> descriptors)
        {
            ArgumentException.ThrowIfNullOrEmpty(shapeType);

            ShapeType = shapeType;

            // Pre-calculate as much as we can for performance reasons.
            foreach (var alterationKey in alterationKeys)
            {
                if (!descriptors.TryGetValue(alterationKey, out var alternationDescriptor))
                {
                    continue;
                }

                _alternationDescriptors.Add(alternationDescriptor);
                _wrappers.AddRange(alternationDescriptor.Wrappers);
                _bindingSources.AddRange(alternationDescriptor.BindingSources);
                _creatingAsync.AddRange(alternationDescriptor.CreatingAsync);
                _createdAsync.AddRange(alternationDescriptor.CreatedAsync);
                _displayingAsync.AddRange(alternationDescriptor.DisplayingAsync);
                _displayedAsync.AddRange(alternationDescriptor.DisplayedAsync);
                _processingAsync.AddRange(alternationDescriptor.ProcessingAsync);

                foreach (var binding in alternationDescriptor.Bindings)
                {
                    _bindings[binding.Key] = binding.Value;
                }
            }
        }

        /// <summary>
        /// The BindingSource is informational text about the source of the Binding delegate. Not used except for
        /// troubleshooting.
        /// </summary>
        public override string BindingSource =>
            Bindings.TryGetValue(ShapeType, out var binding) ? binding.BindingSource : null;

        public override Func<DisplayContext, Task<IHtmlContent>> Binding =>
            Bindings.TryGetValue(ShapeType, out var binding) ? binding.BindingAsync : null;

        public override IDictionary<string, ShapeBinding> Bindings => _bindings;

        public override IReadOnlyList<Func<ShapeCreatingContext, Task>> CreatingAsync => _creatingAsync;

        public override IReadOnlyList<Func<ShapeCreatedContext, Task>> CreatedAsync => _createdAsync;

        public override IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayingAsync => _displayingAsync;

        public override IReadOnlyList<Func<ShapeDisplayContext, Task>> ProcessingAsync => _processingAsync;

        public override IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayedAsync => _displayedAsync;

        public override Func<ShapePlacementContext, PlacementInfo> Placement => CalculatePlacement;

        private PlacementInfo CalculatePlacement(ShapePlacementContext ctx)
        {
            PlacementInfo info = null;
            for (var i = _alternationDescriptors.Count - 1; i >= 0; i--)
            {
                var descriptor = _alternationDescriptors[i];
                info = descriptor.Placement(ctx);
                if (info != null)
                {
                    break;
                }
            }

            return info ?? DefaultPlacementAction(ctx);
        }

        public override IReadOnlyList<string> Wrappers => _wrappers;

        public override IReadOnlyList<string> BindingSources => _bindingSources;
    }

    public class OriginalShapeDescriptor
    {
        public OriginalShapeDescriptor()
        {
            Placement = DefaultPlacementAction;
        }

        protected PlacementInfo DefaultPlacementAction(ShapePlacementContext context)
        {
            // A null default placement means no default placement is specified
            if (DefaultPlacement == null)
            {
                return null;
            }

            return new PlacementInfo
            {
                Location = DefaultPlacement,
            };
        }

        public string ShapeType { get; set; }

        /// <summary>
        /// The BindingSource is informational text about the source of the Binding delegate. Not used except for
        /// troubleshooting.
        /// </summary>
        public virtual string BindingSource =>
            Bindings.TryGetValue(ShapeType, out var binding) ? binding.BindingSource : null;

        public virtual Func<DisplayContext, Task<IHtmlContent>> Binding =>
            Bindings[ShapeType].BindingAsync;

        public virtual IDictionary<string, ShapeBinding> Bindings { get; } = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase);

        public virtual IReadOnlyList<Func<ShapeCreatingContext, Task>> CreatingAsync { get; set; } = [];
        public virtual IReadOnlyList<Func<ShapeCreatedContext, Task>> CreatedAsync { get; set; } = [];
        public virtual IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayingAsync { get; set; } = [];
        public virtual IReadOnlyList<Func<ShapeDisplayContext, Task>> ProcessingAsync { get; set; } = [];
        public virtual IReadOnlyList<Func<ShapeDisplayContext, Task>> DisplayedAsync { get; set; } = [];

        public virtual Func<ShapePlacementContext, PlacementInfo> Placement { get; set; }
        public string DefaultPlacement { get; set; }

        public virtual IReadOnlyList<string> Wrappers { get; set; } = [];
        public virtual IReadOnlyList<string> BindingSources { get; set; } = [];
    }
}
