using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;
using OrchardCore.Localization;
using OrchardCore.Taxonomies.Fields;

namespace OrchardCore.Taxonomies.Liquid
{
    public class TaxonomyTermsFilter : ILiquidFilter
    {
        private readonly ILocalizationEntries _localizationEntries;
        private readonly ILocalizationService _localizationService;

        public TaxonomyTermsFilter(
            ILocalizationEntries localizationEntries,
            ILocalizationService localizationService)
        {
            _localizationEntries = localizationEntries;
            _localizationService = localizationService;
        }        
        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'taxonomy_terms'");
            }

            var contentManager = ((IServiceProvider)services).GetRequiredService<IContentManager>();

            string taxonomyContentItemId = null;
            string[] termContentItemIds = null;
            string localizedCulture = null;

            if (input.Type == FluidValues.Object && input.ToObjectValue() is TaxonomyField field)
            {
                taxonomyContentItemId = field.TaxonomyContentItemId;
                termContentItemIds = field.TermContentItemIds;
            }
            else if (input.Type == FluidValues.Object
                && input.ToObjectValue() is JObject jobj
                && jobj.ContainsKey(nameof(TaxonomyField.TermContentItemIds))
                && jobj.ContainsKey(nameof(TaxonomyField.TaxonomyContentItemId)))
            {
                taxonomyContentItemId = jobj["TaxonomyContentItemId"].Value<string>();
                termContentItemIds = ((JArray)jobj["TermContentItemIds"]).Values<string>().ToArray();
                localizedCulture = jobj.Root["LocalizationPart"]["Culture"].Value<string>();
            }
            else if (input.Type == FluidValues.Array)
            {
                taxonomyContentItemId = arguments.At(0).ToStringValue();
                termContentItemIds = input.Enumerate().Select(x => x.ToStringValue()).ToArray();
            }
            else
            {
                return NilValue.Instance;
            }

            var taxonomy = await contentManager.GetAsync(taxonomyContentItemId);

            if (taxonomy == null)
            {
                return null;
            }

            JArray taxonomyTerms = taxonomy.Content.TaxonomyPart.Terms;
            if(!String.IsNullOrEmpty(localizedCulture))
            {
                if (_localizationEntries.TryGetLocalization(taxonomyContentItemId, out var initialLocalization))
                {
                    if (initialLocalization.Culture.ToLowerInvariant() != localizedCulture.ToLowerInvariant())
                    {
                        var localizations = _localizationEntries.GetLocalizations(initialLocalization.LocalizationSet);
                        foreach (var localization in localizations)
                        {
                            if (localization.Culture.ToLowerInvariant() == localizedCulture.ToLowerInvariant())
                            {
                                taxonomyContentItemId = localization.ContentItemId;
                                var localizedTaxonomy = await contentManager.GetAsync(taxonomyContentItemId);
                                taxonomyTerms.Merge(localizedTaxonomy.Content.TaxonomyPart.Terms);
                            }
                        }
                    }
                }
            }
            var terms = new List<ContentItem>();

            foreach (var termContentItemId in termContentItemIds)
            {
                var term = TaxonomyOrchardHelperExtensions.FindTerm(taxonomyTerms, termContentItemId);

                if (term != null)
                {
                    terms.Add(term);
                }
            }

            return FluidValue.Create(terms);
        }
    }
}
