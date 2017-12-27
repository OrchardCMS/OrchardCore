using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Abstractions.Display
{
    public abstract class ActivityDisplayDriver<T> : DisplayDriver<IActivity, T> where T : class, IActivity
    {
        protected virtual ShapeResult Shape(string shapeType, T activity)
        {
            return Shape(shapeType, shape =>
            {
                shape.Activity = activity;
            });
        }
    }
}
