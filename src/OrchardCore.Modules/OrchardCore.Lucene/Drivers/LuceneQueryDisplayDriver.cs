using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lucene.ViewModels;
using OrchardCore.Queries;

namespace OrchardCore.Lucene.Drivers
{
    public class LuceneQueryDisplayDriver : DisplayDriver<Query, LuceneQuery>
    {
        private IStringLocalizer _stringLocalizer;
        private LuceneIndexManager _luceneIndexManager;

        public LuceneQueryDisplayDriver(
            IStringLocalizer<LuceneQueryDisplayDriver> stringLocalizer,
            LuceneIndexManager luceneIndexManager)
        {
            _luceneIndexManager = luceneIndexManager;
            _stringLocalizer = stringLocalizer;
        }

        public override IDisplayResult Display(LuceneQuery query, IUpdateModel updater)
        {
            return Combine(
                Shape("LuceneQuery_SummaryAdmin", model => { model.Query = query; }).Location("Content:5"),
                Shape("LuceneQuery_Buttons_SummaryAdmin", model => { model.Query = query; }).Location("Actions:2")
            );
        }

        public override IDisplayResult Edit(LuceneQuery query, IUpdateModel updater)
        {
            return Shape<LuceneQueryViewModel>("LuceneQuery_Edit", model =>
            {
                model.Query = query.Template;
                model.Index = query.Index;
                model.ReturnContentItems = query.ReturnContentItems;
                model.Indices = _luceneIndexManager.List().ToArray();

                // Extract query from the query string if we come from the main query editor
                if (string.IsNullOrEmpty(query.Template))
                {
                    updater.TryUpdateModelAsync(model, "", m => m.Query);
                }
            }).Location("Content:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(LuceneQuery model, IUpdateModel updater)
        {
            var viewModel = new LuceneQueryViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix, m => m.Query, m => m.Index, m => m.ReturnContentItems))
            {
                model.Template = viewModel.Query;
                model.Index = viewModel.Index;
                model.ReturnContentItems = viewModel.ReturnContentItems;
            }

            return Edit(model);
        }
    }
}
