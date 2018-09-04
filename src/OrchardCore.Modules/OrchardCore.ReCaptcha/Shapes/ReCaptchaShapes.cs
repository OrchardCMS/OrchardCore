using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.ReCaptcha.Shapes
{
    public class ReCaptchaShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder
                .Describe("ReCaptcha");
        }
    }
}
