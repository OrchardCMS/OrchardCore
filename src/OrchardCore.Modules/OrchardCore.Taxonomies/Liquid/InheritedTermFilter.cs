using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Taxonomies.Helper;

namespace OrchardCore.Taxonomies.Liquid
{
    public class InheritedTermsFilter : ILiquidFilter
    {
        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'inherited_terms'");
            }

            ContentItem taxonomy = null;
            string termContentItemId = null;

            var contentManager = ((IServiceProvider)services).GetRequiredService<IContentManager>();

            if (input.Type == FluidValues.Object && input.ToObjectValue() is ContentItem term)
            {
                termContentItemId = term.ContentItemId;
            }
            else
            {
                termContentItemId = input.ToStringValue();
            }

            var firstArg = arguments.At(0);

            if (firstArg.Type == FluidValues.Object && input.ToObjectValue() is ContentItem contentItem)
            {
                taxonomy = contentItem;
            }
            else
            {
                taxonomy = await contentManager.GetAsync(firstArg.ToStringValue());
            }

            if (taxonomy == null)
            {
                return null;
            }

            var terms = new List<ContentItem>();

            TaxonomyOrchardHelperExtensions.FindTermHierarchy(taxonomy.Content.TaxonomyPart.Terms as JArray, termContentItemId, terms);

            return FluidValue.Create(terms);
        }
    }
}
