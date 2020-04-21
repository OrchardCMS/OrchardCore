using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;
using OrchardCore.Localization;

namespace OrchardCore.DisplayManagement.Liquid
{
    /// <summary>
    /// Provides access to the Culture property.
    /// </summary>
    public class CultureLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        static CultureLiquidTemplateEventHandler()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<CultureInfo, FluidValue>((culture, name) =>
            {
                switch (name)
                {
                    case "Name": return new StringValue(culture.Name);
                    case "Dir": return new StringValue(culture.GetLanguageDirection());

                    default: return null;
                }
            });
        }

        public Task RenderingAsync(TemplateContext context)
        {
            context.SetValue("Culture", CultureInfo.CurrentUICulture);

            return Task.CompletedTask;
        }
    }
}
