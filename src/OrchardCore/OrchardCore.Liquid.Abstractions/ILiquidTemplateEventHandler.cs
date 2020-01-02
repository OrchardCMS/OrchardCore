using System.Threading.Tasks;

namespace OrchardCore.Liquid
{
    public interface ILiquidTemplateEventHandler
    {
        Task RenderingAsync(LiquidTemplateContext context);
    }
}
