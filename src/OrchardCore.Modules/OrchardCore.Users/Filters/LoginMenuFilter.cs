using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;

namespace OrchardCore.Users.Filters;

public class LoginMenuFilter : IAsyncResultFilter
{
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IShapeFactory _shapeFactory;

    public LoginMenuFilter(
        ILayoutAccessor layoutAccessor,
        IShapeFactory shapeFactory)
    {
        _layoutAccessor = layoutAccessor;
        _shapeFactory = shapeFactory;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.IsViewOrPageResult())
        {
            var layout = await _layoutAccessor.GetLayoutAsync();

            var userMenuItemZone = layout.Zones["UserMenuItems"];
            await userMenuItemZone.AddAsync(await _shapeFactory.CreateAsync("TwoFactorUserMenuItem"));
        }

        await next();
    }
}
