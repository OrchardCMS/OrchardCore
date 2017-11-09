using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Queries.ViewModels;

namespace OrchardCore.Queries.Drivers
{
    public class QueryDisplayDriver : DisplayDriver<Query>
    {
        private readonly IStringLocalizer<QueryDisplayDriver> S;

        public QueryDisplayDriver(IStringLocalizer<QueryDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Display(Query query, IUpdateModel updater)
        {
            return Combine(
                Shape("Query_Fields_SummaryAdmin", model =>
                {
                    model.Name = query.Name;
                    model.Source = query.Source;
                    model.Schema = query.Schema;
                    model.Query = query;

                    return Task.CompletedTask;
                }).Location("Content:1"),
                Shape("Query_Buttons_SummaryAdmin", model =>
                {
                    model.Name = query.Name;
                    model.Source = query.Source;
                    model.Schema = query.Schema;
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
                    model.Schema = query.Schema;
                    model.Query = query;
                }).Location("Content:1"),
                Shape<EditQueryViewModel>("Query_Fields_Buttons", model =>
                {
                    model.Name = query.Name;
                    model.Source = query.Source;
                    model.Schema = query.Schema;
                    model.Query = query;
                }).Location("Actions:5")
            );
        }

        public override async Task<IDisplayResult> UpdateAsync(Query model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, m => m.Name, m => m.Source);

            if (String.IsNullOrEmpty(model.Name))
            {
                updater.ModelState.AddModelError(nameof(model.Name), S["Name is required"]);
            }

            return await EditAsync(model, updater);
        }
    }
}
