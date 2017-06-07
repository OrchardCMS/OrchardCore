using DotLiquid;

namespace Orchard.Liquid
{
    public interface ILiquidManager
    {
        Template GetTemplate(string template);
    }
}
