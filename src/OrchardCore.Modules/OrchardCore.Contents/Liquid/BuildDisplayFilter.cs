using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid
{
    public class BuildDisplayFilter : ILiquidFilter
    {
        private const int DefaultMaxContentItemRecursions = 20;

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            static async ValueTask<FluidValue> Awaited(Task<IShape> task)
            {
                return FluidValue.Create(await task);
            }

            var obj = input.ToObjectValue();

            if (!(obj is ContentItem contentItem))
            {
                contentItem = null;

                if (obj is JObject jObject)
                {
                    contentItem = jObject.ToObject<ContentItem>();
                }
            }

            // If input is a 'JObject' but which not represents a 'ContentItem',
            // a 'ContentItem' is still created but with some null properties.
            if (contentItem?.ContentItemId == null)
            {
                return new ValueTask<FluidValue>(NilValue.Instance);
            }

            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'shape_build_display'");
            }

            var serviceProvider = (IServiceProvider)services;

            var buildDisplayRecursionHelper = serviceProvider.GetRequiredService<IContentItemRecursionHelper<BuildDisplayFilter>>();

            // When {{ Model.ContentItem | shape_build_display | shape_render }} is called prevent unlimited recursions.
            // max_recursions is an optional argument to override the default limit of 20.
            var maxRecursions = arguments["max_recursions"];
            var recursionLimit = maxRecursions.Type == FluidValues.Number ? Convert.ToInt32(maxRecursions.ToNumberValue()) : DefaultMaxContentItemRecursions;
            if (buildDisplayRecursionHelper.IsRecursive(contentItem, recursionLimit))
            {
                return new ValueTask<FluidValue>(NilValue.Instance);
            }

            var displayType = arguments["type"].Or(arguments.At(0)).ToStringValue();
            var displayManager = serviceProvider.GetRequiredService<IContentItemDisplayManager>();
            var updateModelAccessor = serviceProvider.GetRequiredService<IUpdateModelAccessor>();

            var task = displayManager.BuildDisplayAsync(contentItem, updateModelAccessor.ModelUpdater, displayType);
            if (task.IsCompletedSuccessfully)
            {
                return new ValueTask<FluidValue>(FluidValue.Create(task.Result));
            }

            return Awaited(task);
        }
    }
}
