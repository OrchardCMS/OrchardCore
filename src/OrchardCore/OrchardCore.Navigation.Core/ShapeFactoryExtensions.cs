using System.Threading.Tasks;
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
}
