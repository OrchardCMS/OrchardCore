using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Implementation
{
    public interface IShapeDisplayEvents
    {
        Task DisplayingAsync(ShapeDisplayContext context);
        Task DisplayedAsync(ShapeDisplayContext context);
        /// <summary>
        /// Guaranteed to be called, even in the event of an exception when rendering the shape.
        /// </summary>
        Task DisplayingFinalizedAsync(ShapeDisplayContext context);
    }
}
