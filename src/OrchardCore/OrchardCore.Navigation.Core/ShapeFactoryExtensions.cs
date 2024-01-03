using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Navigation;

namespace OrchardCore.DisplayManagement.Extensions;

public static class ShapeFactoryExtensions
{
    public static ValueTask<IShape> PagerAsync(this IShapeFactory _shapeFactory, Pager pager, int totalItemCount)
        => _shapeFactory.CreateAsync("Pager", Arguments.From(new
        {
            pager.Page,
            pager.PageSize,
            TotalItemCount = totalItemCount,
        }));

    public static async ValueTask<IShape> PagerAsync(this IShapeFactory _shapeFactory, Pager pager, int totalItemCount, RouteData routeData)
    {
        dynamic pagerShape = await _shapeFactory.PagerAsync(pager, totalItemCount);

        if (routeData != null)
        {
            pagerShape.RouteData(routeData);
        }

        return pagerShape;
    }

    public static ValueTask<IShape> PagerAsync(this IShapeFactory _shapeFactory, Pager pager, int totalItemCount, RouteValueDictionary routeValues)
        => _shapeFactory.PagerAsync(pager, totalItemCount, routeValues == null ? null : new RouteData(routeValues));
}
