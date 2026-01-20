using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.LocationExpander;

namespace OrchardCore.Mvc
{
    public class ModularRazorViewEngineOptionsSetup : IConfigureOptions<RazorViewEngineOptions>
    {
        public ModularRazorViewEngineOptionsSetup()
        {
        }

        public void Configure(RazorViewEngineOptions options)
        {
            options.ViewLocationExpanders.Add(new CompositeViewLocationExpanderProvider());
        }
    }
}
