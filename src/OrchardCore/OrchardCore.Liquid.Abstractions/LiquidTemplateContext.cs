using System;
using Fluid;

namespace OrchardCore.Liquid
{
    public class LiquidTemplateContext : TemplateContext
    {
        public delegate void EnterScopeHandler(LiquidTemplateContext context, object model);

        private EnterScopeHandler _enterScope;

        public LiquidTemplateContext(IServiceProvider services)
        {
            Services = services;
        }

        public bool IsInitialized { get; set; }

        public IServiceProvider Services { get; }

        /// <summary>
        /// Add an handler that will be executed when the <see cref="LiquidTemplateContext"/> enters in a new child scope.
        /// </summary>
        /// <param name="handler"></param>
        public void OnEnterScope(EnterScopeHandler handler) => _enterScope += handler;

        public void EnterScope(object model)
        {
            EnterChildScope();

            _enterScope?.Invoke(this, model);
        }
    }
}
