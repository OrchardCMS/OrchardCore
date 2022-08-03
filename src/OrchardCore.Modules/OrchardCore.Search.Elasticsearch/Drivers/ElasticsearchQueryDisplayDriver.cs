using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Queries;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch.Drivers
{
    public class ElasticsearchQueryDisplayDriver : DisplayDriver<Query, ElasticsearchQuery>
    {
        private readonly ElasticsearchIndexSettingsService _elasticIndexSettingsService;
        private readonly IStringLocalizer S;

        public ElasticsearchQueryDisplayDriver(
            IStringLocalizer<ElasticsearchQueryDisplayDriver> stringLocalizer,
            ElasticsearchIndexSettingsService elasticIndexSettingsService)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
            S = stringLocalizer;
        }

        public override IDisplayResult Display(ElasticsearchQuery query, IUpdateModel updater)
        {
            return Combine(
                Dynamic("ElasticsearchQuery_SummaryAdmin", model => { model.Query = query; }).Location("Content:5"),
                Dynamic("ElasticsearchQuery_Buttons_SummaryAdmin", model => { model.Query = query; }).Location("Actions:2")
            );
        }

        public override IDisplayResult Edit(ElasticsearchQuery query, IUpdateModel updater)
        {
            return Initialize<ElasticsearchQueryViewModel>("ElasticsearchQuery_Edit", async model =>
            {
                model.Query = query.Template;
                model.Index = query.Index;
                model.ReturnContentItems = query.ReturnContentItems;
                model.Indices = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();

                // Extract query from the query string if we come from the main query editor
                if (String.IsNullOrEmpty(query.Template))
                {
                    await updater.TryUpdateModelAsync(model, "", m => m.Query);
                }
            }).Location("Content:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(ElasticsearchQuery model, IUpdateModel updater)
        {
            var viewModel = new ElasticsearchQueryViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix, m => m.Query, m => m.Index, m => m.ReturnContentItems))
            {
                model.Template = viewModel.Query;
                model.Index = viewModel.Index;
                model.ReturnContentItems = viewModel.ReturnContentItems;
            }

            if (String.IsNullOrWhiteSpace(model.Template))
            {
                updater.ModelState.AddModelError(nameof(model.Template), S["The query field is required"]);
            }

            if (String.IsNullOrWhiteSpace(model.Index))
            {
                updater.ModelState.AddModelError(nameof(model.Index), S["The index field is required"]);
            }

            return Edit(model, updater);
        }
    }
}
