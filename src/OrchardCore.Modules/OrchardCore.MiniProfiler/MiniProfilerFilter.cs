using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.MiniProfiler
{
    public class MiniProfilerFilter : IAsyncResultFilter
    {
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly IShapeFactory _shapeFactory;
        private readonly MiniProfilerOptions _options;

        public MiniProfilerFilter(
            ILayoutAccessor layoutAccessor,
            IShapeFactory shapeFactory,
            IOptions<MiniProfilerOptions> options)
        {
            _layoutAccessor = layoutAccessor;
            _shapeFactory = shapeFactory;
            _options = options.Value;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Should only run on the front-end (or optionally also on the admin) for a full view.
            if ((context.Result is ViewResult || context.Result is PageResult) &&
                (_options.AllowOnAdmin || !AdminAttribute.IsApplied(context.HttpContext)))
            {
                var layout = await _layoutAccessor.GetLayoutAsync() as ZoneHolding;
                var footerZone = layout["Footer"];

                if (footerZone is ZoneOnDemand zoneOnDemand)
                {
                    await zoneOnDemand.AddAsync(await _shapeFactory.CreateAsync("MiniProfiler"));
                }
                else if (footerZone is Shape shape)
                {
                    shape.Add(await _shapeFactory.CreateAsync("MiniProfiler"));
                }
            }

            await next.Invoke();
        }
    }
}
