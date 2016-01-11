using Orchard.ContentManagement.Display.Handlers;
using Orchard.ContentManagement.Display.Views;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using System;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public class ContentElementDisplay : IContentElementDisplay
    {
        /// <summary>
        /// Creates a new strongly typed shape.
        /// </summary>
        public ContentShapeResult Shape<T>() where T : Shape, new()
        {
            return new ContentShapeResult(typeof(T).Name, ctx => ctx.ShapeFactory.Create<T>());
        }

        /// <summary>
        /// Creates a new strongly typed shape and initializes it if it needs to be rendered.
        /// </summary>
        public ContentShapeResult Shape<T>(Func<T, Task> initialize) where T : Shape, new()
        {
            return new ContentShapeResult(
                typeof(T).Name, 
                ctx => ctx.ShapeFactory.Create<T>(),
                shape => initialize((T)shape)
                );
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name.
        /// </summary>
        public ContentShapeResult Shape(string shapeType)
        {
            return new ContentShapeResult(shapeType, ctx => ctx.ShapeFactory.Create(shapeType));
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name.
        /// </summary>
        public ContentShapeResult Shape(string shapeType, Func<dynamic, Task> initialize)
        {
            return new ContentShapeResult(
                shapeType, 
                ctx => ctx.ShapeFactory.Create(shapeType),
                initialize
                );
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created automatically from its type name and initialized with a model.
        /// </summary>
        public ContentShapeResult Shape(string shapeType, object model)
        {
            return new ContentShapeResult(shapeType, ctx => ctx.ShapeFactory.Create(shapeType, Arguments.From(model)));
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        public ContentShapeResult Shape(string shapeType, Func<BuildShapeContext, dynamic> shapeBuilder)
        {
            return new ContentShapeResult(shapeType, shapeBuilder);
        }

        /// <summary>
        /// If the shape needs to be rendered, it is created by the delegate.
        /// </summary>
        public ContentShapeResult Shape(string shapeType, Func<BuildShapeContext, dynamic> shapeBuilder, Func<dynamic, Task> initialize)
        {
            return new ContentShapeResult(shapeType, shapeBuilder, initialize);
        }

        public CombinedResult Combine(params DisplayResult[] results)
        {
            return new CombinedResult(results);
        }

        public virtual DisplayResult BuildDisplay(BuildDisplayContext context)
        {
            return null;
        }

        public virtual DisplayResult BuildEditor(BuildEditorContext context)
        {
            return null;
        }

        public virtual DisplayResult UpdateEditor(UpdateEditorContext context)
        {
            return null;
        }
    }
}
