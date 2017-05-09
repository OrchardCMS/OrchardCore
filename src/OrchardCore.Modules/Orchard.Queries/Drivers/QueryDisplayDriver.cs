using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Queries.ViewModels;

namespace Orchard.Queries.Drivers
{
    public class QueryDisplayDriver : DisplayDriver<Query>
    {
        public override IDisplayResult Display(Query query, IUpdateModel updater)
        {
            return Combine(
                Shape("Query_Fields_SummaryAdmin", model =>
                {
                    model.Name = query.Name;
                    model.Source = query.Source;
                    model.Query = query;

                    return Task.CompletedTask;
                }).Location("Content:1"),
                Shape("Query_Buttons_SummaryAdmin", model =>
                {
                    model.Name = query.Name;
                    model.Source = query.Source;
                    model.Query = query;

                    return Task.CompletedTask;
                }).Location("Actions:5")
            );
        }

        public override IDisplayResult Edit(Query query, IUpdateModel updater)
        {
            return Combine(
                Shape<EditQueryViewModel>("Query_Fields_Edit", model =>
                {
                    model.Name = query.Name;
                    model.Source = query.Source;
                    model.Query = query;
                }).Location("Content:1"),
                Shape<EditQueryViewModel>("Query_Fields_Buttons", model =>
                {
                    model.Name = query.Name;
                    model.Source = query.Source;
                    model.Query = query;
                }).Location("Actions:5")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(Query model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, m => m.Name, m => m.Source);

            // TODO: Validate name is unique and not empty

            return await EditAsync(model, updater);
        }
    }
}
