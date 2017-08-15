using Orchard.DisplayManagement.Razor;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewExtensionProvider : IRazorViewExtensionProvider
    {
        public string ViewExtension => FluidViewTemplate.ViewExtension;
    }
}
