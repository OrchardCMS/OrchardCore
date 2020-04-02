using System;
using Fluid;

namespace OrchardCore.Liquid
{
    public class LiquidTemplateContext : TemplateContext
    {
        public const int MaxShapeRecursions = 3;

        public LiquidTemplateContext(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }
    }
}
