using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Queries.Sql.ViewModels;

namespace Orchard.Queries.Sql.Drivers
{
    public class SqlQueryDisplayDriver : DisplayDriver<Query, SqlQuery>
    {
        private IStringLocalizer _stringLocalizer;

        public SqlQueryDisplayDriver(IStringLocalizer<SqlQueryDisplayDriver> stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        public override IDisplayResult Display(SqlQuery query, IUpdateModel updater)
        {
            return Combine(
                Shape("SqlQuery_SummaryAdmin", model =>
                {
                    model.Query = query;

                    return Task.CompletedTask;
                }).Location("Content:5"),
                Shape("SqlQuery_Buttons_SummaryAdmin", model =>
                {
                    model.Query = query;

                    return Task.CompletedTask;
                }).Location("Actions:2")
            );
        }

        public override IDisplayResult Edit(SqlQuery query, IUpdateModel updater)
        {
            return Shape<SqlQueryViewModel>("SqlQuery_Edit", model =>
            {
                model.Query = query.Template;
                model.ReturnContentItems = query.ReturnContentItems;

                // Extract query from the query string if we come from the main query editor
                if (string.IsNullOrEmpty(query.Template))
                {
                    updater.TryUpdateModelAsync(model, "", m => m.Query);
                }

            }).Location("Content:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(SqlQuery model, IUpdateModel updater)
        {
            var viewModel = new SqlQueryViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix, m => m.Query, m => m.ReturnContentItems))
            {
                model.Template = viewModel.Query;
                model.ReturnContentItems = viewModel.ReturnContentItems;
            }

            return Edit(model);
        }
    }
}
