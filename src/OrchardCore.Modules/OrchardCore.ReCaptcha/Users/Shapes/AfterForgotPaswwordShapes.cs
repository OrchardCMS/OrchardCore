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
    public class AfterForgotPasswordShapes : IShapeFactoryEvents
    {
        public AfterForgotPasswordShapes()
        {
            
        }

        public async void Created(ShapeCreatedContext context)
        {
            if (context.ShapeType == "AfterForgotPassword")
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
