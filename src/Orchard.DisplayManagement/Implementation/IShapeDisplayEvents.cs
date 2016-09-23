using Orchard.DisplayManagement.Shapes;
using Microsoft.AspNetCore.Html;

namespace Orchard.DisplayManagement.Implementation
{
    public interface IShapeDisplayEvents
    {
        void Displaying(ShapeDisplayingContext context);
        void Displayed(ShapeDisplayedContext context);
    }

    public class ShapeDisplayingContext
    {
        public dynamic Shape { get; set; }
        public ShapeMetadata ShapeMetadata { get; set; }
        public IHtmlContent ChildContent { get; set; }
        public DisplayContext DisplayContext { get; set; }
    }

    public class ShapeDisplayedContext
    {
        public dynamic Shape { get; set; }
        public ShapeMetadata ShapeMetadata { get; set; }
        public IHtmlContent ChildContent { get; set; }
        public DisplayContext DisplayContext { get; set; }
    }

    public abstract class ShapeDisplayEvents : IShapeDisplayEvents
    {
        public virtual void Displaying(ShapeDisplayingContext context) { }
        public virtual void Displayed(ShapeDisplayedContext context) { }
    }
}