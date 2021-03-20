using System;
using System.Threading.Tasks;
using Fluid.Values;

namespace OrchardCore.Liquid
{
    /// <summary>
    /// Can be used to provide a factory to return a value based on a property name
    /// that is unknown at registration time.
    ///
    /// e.g. {{ LiquidPropertyAccessor.MyPropertyName }} (MyPropertyName will be passed as the identifier argument to the factory)
    /// </summary>
    public class LiquidPropertyAccessor
    {
        private readonly Func<string, LiquidTemplateContext, Task<FluidValue>> _getter;
        private readonly LiquidTemplateContext _context;

        public LiquidPropertyAccessor(LiquidTemplateContext context, Func<string, LiquidTemplateContext, Task<FluidValue>> getter)
        {
            _getter = getter;
            _context = context;
        }

        public Task<FluidValue> GetValueAsync(string identifier)
        {
            return _getter(identifier, _context);
        }
    }
}
