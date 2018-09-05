using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.ReCaptcha.Core.Services;
using OrchardCore.Users.Shapes;

namespace OrchardCore.ReCaptcha.Users.Shapes
{
    public class AfterLoginShapes : IShapeFactoryEvents
    {
        private readonly IReCaptchaService _captchaService;

        public AfterLoginShapes(IReCaptchaService captchaService)
        {
            _captchaService = captchaService;
        }

        public async void Created(ShapeCreatedContext context)
        {
            if (context.ShapeType == "AfterLogin" && await _captchaService.IsConvictedAsync())
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
