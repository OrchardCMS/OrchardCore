using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.MiniProfiler
{
    public class MiniProfilerFilter : IAsyncResultFilter
    {
        private readonly ILayoutAccessor _layoutAccessor;
        private readonly IShapeFactory _shapeFactory;
        private readonly IAuthorizationService _authorizationService;

        public MiniProfilerFilter(
            ILayoutAccessor layoutAccessor,
            IShapeFactory shapeFactory,
            IAuthorizationService authorizationService)
        {
            _layoutAccessor = layoutAccessor;
            _shapeFactory = shapeFactory;
            _authorizationService = authorizationService;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var viewMiniProfilerOnFrontEnd = await _authorizationService.AuthorizeAsync(context.HttpContext.User, Permissions.ViewMiniProfilerOnFrontEnd);
            var viewMiniProfilerOnBackEnd = await _authorizationService.AuthorizeAsync(context.HttpContext.User, Permissions.ViewMiniProfilerOnBackEnd);
            if (
                    (context.Result is ViewResult || context.Result is PageResult) &&
                    (
                        (viewMiniProfilerOnFrontEnd && !AdminAttribute.IsApplied(context.HttpContext)) ||
                        (viewMiniProfilerOnBackEnd && AdminAttribute.IsApplied(context.HttpContext))
                    )
                )
            {
                var layout = await _layoutAccessor.GetLayoutAsync();
                var footerZone = layout.Zones["Footer"];

                if (footerZone is Shape shape)
                {
                    await shape.AddAsync(await _shapeFactory.CreateAsync("MiniProfiler"));
                }
            }

            await next.Invoke();
        }
    }
}
