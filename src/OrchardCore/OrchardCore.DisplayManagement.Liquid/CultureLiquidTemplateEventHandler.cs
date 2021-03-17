using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid
{
    /// <summary>
    /// Provides access to the Culture property.
    /// </summary>
    public class CultureLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        public Task RenderingAsync(TemplateContext context)
        {
            context.SetValue("Culture", CultureInfo.CurrentUICulture);

            return Task.CompletedTask;
        }
    }
}
