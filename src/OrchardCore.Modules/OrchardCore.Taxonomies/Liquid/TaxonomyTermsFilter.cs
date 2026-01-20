using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Taxonomies.Fields;

namespace OrchardCore.Taxonomies.Liquid;

public class TaxonomyTermsFilter : ILiquidFilter
{
    private readonly IContentManager _contentManager;

    public TaxonomyTermsFilter(IContentManager contentManager)
    {
        _contentManager = contentManager;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
    {
        string taxonomyContentItemId = null;
        string[] termContentItemIds = null;

        if (input.Type == FluidValues.Object && input.ToObjectValue() is TaxonomyField field)
        {
            taxonomyContentItemId = field.TaxonomyContentItemId;
            termContentItemIds = field.TermContentItemIds;
        }
        else if (input.Type == FluidValues.Object
            && input.ToObjectValue() is JsonObject jobj
            && jobj.ContainsKey(nameof(TaxonomyField.TermContentItemIds))
            && jobj.ContainsKey(nameof(TaxonomyField.TaxonomyContentItemId)))
        {
            taxonomyContentItemId = jobj["TaxonomyContentItemId"].Value<string>();
            termContentItemIds = jobj["TermContentItemIds"].Values<string>().ToArray();
        }
        else if (input.Type == FluidValues.Array)
        {
            taxonomyContentItemId = arguments.At(0).ToStringValue();
            termContentItemIds = input.Enumerate(ctx).Select(x => x.ToStringValue()).ToArray();
        }
        else
        {
            return NilValue.Instance;
        }

        var taxonomy = await _contentManager.GetAsync(taxonomyContentItemId);

        if (taxonomy == null)
        {
            return null;
        }

        var terms = new List<ContentItem>();

        foreach (var termContentItemId in termContentItemIds)
        {
            var term = TaxonomyOrchardHelperExtensions.FindTerm(
                (JsonArray)taxonomy.Content["TaxonomyPart"]["Terms"],
                termContentItemId);

            if (term is not null)
            {
                terms.Add(term);
            }
        }

        return FluidValue.Create(terms, ctx.Options);
    }
}
