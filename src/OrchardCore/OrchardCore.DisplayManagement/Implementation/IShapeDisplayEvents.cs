using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Implementation
{
    public interface IShapeDisplayEvents
    {
        Task DisplayingAsync(ShapeDisplayContext context);
        Task DisplayedAsync(ShapeDisplayContext context);
    }
}