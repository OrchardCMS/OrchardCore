using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Lucene.ViewModels;
using Orchard.Queries;

namespace Orchard.Lucene.Drivers
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
                Shape("LuceneQuery_SummaryAdmin", model =>
                {
                    model.Query = query;

                    return Task.CompletedTask;
                }).Location("Content:5"),
                Shape("LuceneQuery_Buttons_SummaryAdmin", model =>
                {
                    model.Query = query;

                    return Task.CompletedTask;
                }).Location("Actions:2")
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
