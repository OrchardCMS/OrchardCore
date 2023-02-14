using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
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

        private readonly IContentItemRecursionHelper<BuildDisplayFilter> _buildDisplayRecursionHelper;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public BuildDisplayFilter(IContentItemRecursionHelper<BuildDisplayFilter> buildDisplayRecursionHelper,
            IContentItemDisplayManager contentItemDisplayManager,
            IUpdateModelAccessor updateModelAccessor)
        {
            _buildDisplayRecursionHelper = buildDisplayRecursionHelper;
            _contentItemDisplayManager = contentItemDisplayManager;
            _updateModelAccessor = updateModelAccessor;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            static async ValueTask<FluidValue> Awaited(Task<IShape> task, TemplateOptions options)
            {
                return FluidValue.Create(await task, options);
            }

            var obj = input.ToObjectValue();

            if (obj is not ContentItem contentItem)
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

            // When {{ Model.ContentItem | shape_build_display | shape_render }} is called prevent unlimited recursions.
            // max_recursions is an optional argument to override the default limit of 20.
            var maxRecursions = arguments["max_recursions"];
            var recursionLimit = maxRecursions.Type == FluidValues.Number ? Convert.ToInt32(maxRecursions.ToNumberValue()) : DefaultMaxContentItemRecursions;
            if (_buildDisplayRecursionHelper.IsRecursive(contentItem, recursionLimit))
            {
                return new ValueTask<FluidValue>(NilValue.Instance);
            }

            var displayType = arguments["type"].Or(arguments.At(0)).ToStringValue();

            var task = _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, displayType);
            if (task.IsCompletedSuccessfully)
            {
                return new ValueTask<FluidValue>(FluidValue.Create(task.Result, ctx.Options));
            }

            return Awaited(task, ctx.Options);
        }
    }
}
