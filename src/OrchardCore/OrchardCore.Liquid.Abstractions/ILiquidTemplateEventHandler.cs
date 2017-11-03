using System.Threading.Tasks;
using Fluid;

namespace OrchardCore.Liquid
{
    public interface ILiquidTemplateEventHandler
    {
        Task RenderingAsync(TemplateContext context);
    }
}
