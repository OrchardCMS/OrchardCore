using System;
using Fluid;

namespace OrchardCore.Liquid
{
    public class LiquidTemplateContext : TemplateContext, IDisposable
    {
        public LiquidTemplateContext(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

        public LiquidTemplateContext EnterChildScope(object model)
        {
            if (model != null)
            {
                MemberAccessStrategy.Register(model.GetType());
            }

            EnterChildScope();
            SetValue("Model", model);

            return this;
        }

        public void Dispose()
        {
            ReleaseScope();
        }
    }
}
