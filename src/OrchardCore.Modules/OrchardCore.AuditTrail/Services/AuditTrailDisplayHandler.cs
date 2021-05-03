using System;
using System.Threading.Tasks;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailDisplayHandler : AuditTrailDisplayHandlerBase
    {
        public override async Task DisplayFiltersAsync(DisplayFiltersContext context)
        {
            var fromDateFilterShape = await context.ShapeFactory.CreateAsync<AuditTrailFilterViewModel>("AuditTrailFilter__FromDate", model =>
            {
                model.FromDate = context.Filters.Get("fromDate");
            });

            var toDateFilterShape = await context.ShapeFactory.CreateAsync<AuditTrailFilterViewModel>("AuditTrailFilter__ToDate", model =>
            {
                model.ToDate = context.Filters.Get("toDate");
            });

            var userNameFilterShape = await context.ShapeFactory.CreateAsync<AuditTrailFilterViewModel>("AuditTrailFilter__User", model =>
            {
                model.UserName = context.Filters.Get("username");
            });

            var categoryFilterShape = await context.ShapeFactory.CreateAsync<AuditTrailFilterViewModel>("AuditTrailFilter__Category", model =>
            {
                model.Category = context.Filters.Get("category");
                model.Categories = context.Categories;
            });

            await context.FiltersShape.AddAsync(fromDateFilterShape, String.Empty);
            await context.FiltersShape.AddAsync(toDateFilterShape, String.Empty);
            await context.FiltersShape.AddAsync(categoryFilterShape, String.Empty);
            await context.FiltersShape.AddAsync(userNameFilterShape, String.Empty);
        }
    }
}
