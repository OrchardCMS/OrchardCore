using Orchard.DisplayManagement.Razor;

namespace Orchard.DisplayManagement.Liquid
{
    public class LiquidViewExtensionProvider : IRazorViewExtensionProvider
    {
        public string ViewExtension => LiquidViewTemplate.ViewExtension;
    }
}
