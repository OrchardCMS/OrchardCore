using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.ReCaptcha.Services;

namespace OrchardCore.ReCaptcha.Users.Shapes
{
    public class AfterLoginShapes : IShapeFactoryEvents
    {
        private readonly ReCaptchaService _captchaService;

        public AfterLoginShapes(ReCaptchaService captchaService)
        {
            _captchaService = captchaService;
        }

        public async void Created(ShapeCreatedContext context)
        {
            if (context.ShapeType == "AfterLogin" && _captchaService.IsConvicted())
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
