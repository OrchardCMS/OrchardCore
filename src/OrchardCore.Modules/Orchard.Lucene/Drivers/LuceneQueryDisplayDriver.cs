using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
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

        public LuceneQueryDisplayDriver(IStringLocalizer<LuceneQueryDisplayDriver> stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        public override IDisplayResult Edit(LuceneQuery query, IUpdateModel updater)
        {
            return Shape<LuceneQueryViewModel>("LuceneQuery_Edit", model =>
            {
                model.Query = query.Content?.ToString(Newtonsoft.Json.Formatting.Indented);
                model.Index = query.IndexName;
            }).Location("Content:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(LuceneQuery model, IUpdateModel updater)
        {
            var viewModel = new LuceneQueryViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix, m => m.Index, m => m.Query))
            {
                try
                {
                    model.Content = JObject.Parse(viewModel.Query);
                }
                catch
                {
                    updater.ModelState.AddModelError(nameof(viewModel.Query), _stringLocalizer["Invalid format"]);
                }

                model.IndexName = viewModel.Index;
            }

            return Edit(model);
        }
    }
}
