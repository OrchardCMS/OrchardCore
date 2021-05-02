using System;
using System.Threading.Tasks;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailEventDriver : AuditTrailEventDriverBase
    {
        public override async Task DisplayFiltersAsync(DisplayFiltersContext context)
        {
            var userNameFilterShape = await context.ShapeFactory.CreateAsync("AuditTrailFilter__Common__User", Arguments.From(new
            {
                UserName = context.Filters.Get("username")
            }));

            var dateFromFilterShape = await context.ShapeFactory.CreateAsync("AuditTrailFilter__Common__Date__From", Arguments.From(new
            {
                Date = context.Filters.Get("fromDate")
            }));

            var dateToFilterShape = await context.ShapeFactory.CreateAsync("AuditTrailFilter__Common__Date__To", Arguments.From(new
            {
                Date = context.Filters.Get("toDate")
            }));

            var categoryFilterShape = await context.ShapeFactory.CreateAsync("AuditTrailFilter__Common__Category", Arguments.From(new
            {
                context.Categories,
                Category = context.Filters.Get("category")
            }));

            await context.FiltersShape.AddAsync(dateFromFilterShape, String.Empty);
            await context .FiltersShape.AddAsync(dateToFilterShape, String.Empty);
            await context .FiltersShape.AddAsync(categoryFilterShape, String.Empty);
            await context.FiltersShape.AddAsync(userNameFilterShape, String.Empty);
        }
    }
}
