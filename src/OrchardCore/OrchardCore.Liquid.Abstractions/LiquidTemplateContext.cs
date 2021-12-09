using System;
using Fluid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Liquid
{
    public class LiquidTemplateContext : TemplateContext
    {
        public const int MaxShapeRecursions = 3;

        public LiquidTemplateContext(IServiceProvider services, TemplateOptions options) : base(options)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

        public ViewContext ViewContext { get; set; }

        public bool IsInitialized { get; set; }

        public int ShapeRecursions { get; set; }
    }
}
