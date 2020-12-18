using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Modules.Manifest;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Benchmark
{
    [MemoryDiagnoser]
    public class ShapeFactoryBenchmark
    {
        private static readonly FilterArguments _filterArguments = new FilterArguments().Add("utc", DateTime.UtcNow).Add("format", "MMMM dd, yyyy");
        private static readonly FluidValue input = FluidValue.Create("DateTime");
        private static readonly TemplateContext _templateContext;

        static ShapeFactoryBenchmark()
        {
            _templateContext = new TemplateContext();
            var defaultShapeTable = new ShapeTable
            (
                new Dictionary<string, ShapeDescriptor>(),
                new Dictionary<string, ShapeBinding>()
            );

            var shapeFactory = new DefaultShapeFactory(
                serviceProvider: new ServiceCollection().BuildServiceProvider(),
                events: Enumerable.Empty<IShapeFactoryEvents>(),
                shapeTableManager: new TestShapeTableManager(defaultShapeTable),
                themeManager: new MockThemeManager(new ExtensionInfo("path", new ManifestInfo(new ModuleAttribute()), (x, y) => Enumerable.Empty<IFeatureInfo>())));

            _templateContext.AmbientValues["DisplayHelper"] = new DisplayHelper(null, shapeFactory, null);
        }

        [Benchmark(Baseline = true)]
        public async Task OriginalShapeRender()
        {
            await ShapeRenderOriginal(input, _filterArguments, _templateContext);
        }

        [Benchmark]
        public async Task NewShapeRender()
        {
            await LiquidViewFilters.ShapeRender(input, _filterArguments, _templateContext);
        }

        private static async ValueTask<FluidValue> ShapeRenderOriginal(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'shape_render'");
            }

            if (input.ToObjectValue() is IShape shape)
            {
                return new HtmlContentValue(await (Task<IHtmlContent>)displayHelper(shape));
            }

            return NilValue.Instance;
        }
    }
}
