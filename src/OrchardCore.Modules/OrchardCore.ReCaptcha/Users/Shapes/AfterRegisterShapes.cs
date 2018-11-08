using OrchardCore.DisplayManagement.Implementation;

namespace OrchardCore.ReCaptcha.Users.Shapes
{
    public class AfterRegistrationShapes : IShapeFactoryEvents
    {
        public AfterRegistrationShapes()
        {
            
        }

        public async void Created(ShapeCreatedContext context)
        {
            if (context.ShapeType == "AfterRegister")
            {
                dynamic layout = context.Shape;   
                layout.Add(await context.ShapeFactory.New.ReCaptcha());
            }
        }

        public void Creating(ShapeCreatingContext context)
        {
            // this method is intentionally left blank
        }
    }
}
