using System;
using Fluid;

namespace OrchardCore.Liquid
{
    public class LiquidTemplateContext : TemplateContext
    {
        public LiquidTemplateContext(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }
    }
}
