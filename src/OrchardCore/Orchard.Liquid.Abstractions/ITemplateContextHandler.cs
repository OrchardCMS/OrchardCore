using Fluid;

namespace Orchard.Liquid
{
    public interface ITemplateContextHandler
    {
        void OnTemplateProcessing(TemplateContext context);
    }
}
