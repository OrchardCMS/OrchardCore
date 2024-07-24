using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Queries;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Drivers
{
    public sealed class LuceneQueryDisplayDriver : DisplayDriver<Query>
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

        internal readonly IStringLocalizer S;

        public LuceneQueryDisplayDriver(
            IStringLocalizer<LuceneQueryDisplayDriver> stringLocalizer,
            LuceneIndexSettingsService luceneIndexSettingsService)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
            S = stringLocalizer;
        }

        public override IDisplayResult Display(Query query, IUpdateModel updater)
        {
            if (query.Source != LuceneQuerySource.SourceName)
            {
                return null;
            }

            return Combine(
                Dynamic("LuceneQuery_SummaryAdmin", model => { model.Query = query; }).Location("Content:5"),
                Dynamic("LuceneQuery_Buttons_SummaryAdmin", model => { model.Query = query; }).Location("Actions:2")
            );
        }

        public override IDisplayResult Edit(Query query, IUpdateModel updater)
        {
            if (query.Source != LuceneQuerySource.SourceName)
            {
                return null;
            }

            return Initialize<LuceneQueryViewModel>("LuceneQuery_Edit", async model =>
            {
                var metadata = query.As<LuceneQueryMetadata>();

                model.Query = metadata.Template;
                model.Index = metadata.Index;
                model.ReturnContentItems = query.ReturnContentItems;
                model.Indices = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();

                // Extract query from the query string if we come from the main query editor.
                if (string.IsNullOrEmpty(metadata.Template))
                {
                    await updater.TryUpdateModelAsync(model, string.Empty, m => m.Query);
                }
            }).Location("Content:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(Query query, IUpdateModel updater)
        {
            if (query.Source != LuceneQuerySource.SourceName)
            {
                return null;
            }

            var viewModel = new LuceneQueryViewModel();
            await updater.TryUpdateModelAsync(viewModel, Prefix, m => m.Query, m => m.Index, m => m.ReturnContentItems);

            if (string.IsNullOrWhiteSpace(viewModel.Query))
            {
                updater.ModelState.AddModelError(Prefix, nameof(viewModel.Query), S["The query field is required"]);
            }

            if (string.IsNullOrWhiteSpace(viewModel.Index))
            {
                updater.ModelState.AddModelError(Prefix, nameof(viewModel.Index), S["The index field is required"]);
            }

            query.ReturnContentItems = viewModel.ReturnContentItems;
            query.Put(new LuceneQueryMetadata()
            {
                Template = viewModel.Query,
                Index = viewModel.Index,
            });

            return Edit(query, updater);
        }
    }
}
