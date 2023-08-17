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
        private static readonly FilterArguments _filterArguments = new FilterArguments().Add("utc", new DateTimeValue(DateTime.UtcNow)).Add("format", StringValue.Create("MMMM dd, yyyy"));
        private static readonly FluidValue _input = StringValue.Create("DateTime");
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

        // TODO this benchmark is meaningless as the benchmark noops as the input is not a shape.
        [Benchmark(Baseline = true)]
#pragma warning disable CA1822 // Mark members as static
        public async Task OriginalShapeRender()
#pragma warning restore CA1822 // Mark members as static
        {
            await ShapeRenderOriginal(_input, _filterArguments, _templateContext);
        }


        // [Benchmark]
        // public async Task NewShapeRender()
        // {
        //     await LiquidViewFilters.ShapeRender(input, _filterArguments, _templateContext);
        // }

        private static async ValueTask<FluidValue> ShapeRenderOriginal(FluidValue input, FilterArguments _, TemplateContext context)
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
