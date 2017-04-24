namespace Orchard.DisplayManagement.Implementation
{
    public interface IShapeDisplayEvents
    {
        void Displaying(ShapeDisplayContext context);
        void Displayed(ShapeDisplayContext context);
    }

    public abstract class ShapeDisplayEvents : IShapeDisplayEvents
    {
        public virtual void Displaying(ShapeDisplayContext context) { }
        public virtual void Displayed(ShapeDisplayContext context) { }
    }
}