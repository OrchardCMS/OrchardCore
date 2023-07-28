using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;

namespace OrchardCore.Taxonomies.Liquid
{
    public class InheritedTermsFilter : ILiquidFilter
    {
        private readonly IContentManager _contentManager;

        public InheritedTermsFilter(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            var termContentItemId = input.Type == FluidValues.Object && input.ToObjectValue() is ContentItem term
                ? term.ContentItemId
                : input.ToStringValue();

            var firstArg = arguments.At(0);
            if (firstArg.Type != FluidValues.Object || input.ToObjectValue() is not ContentItem taxonomy)
            {
                taxonomy = await _contentManager.GetAsync(firstArg.ToStringValue());
            }

            if (taxonomy == null)
            {
                return null;
            }

            var terms = new List<ContentItem>();

            TaxonomyOrchardHelperExtensions.FindTermHierarchy(taxonomy.Content.TaxonomyPart.Terms as JArray, termContentItemId, terms);

            return FluidValue.Create(terms, ctx.Options);
        }
    }
}
