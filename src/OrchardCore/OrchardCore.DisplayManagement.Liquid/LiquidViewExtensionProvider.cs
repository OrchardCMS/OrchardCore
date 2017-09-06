using OrchardCore.DisplayManagement.Razor;

namespace OrchardCore.DisplayManagement.Liquid
{
    public class LiquidViewExtensionProvider : IRazorViewExtensionProvider
    {
        public string ViewExtension => LiquidViewTemplate.ViewExtension;
    }
}
