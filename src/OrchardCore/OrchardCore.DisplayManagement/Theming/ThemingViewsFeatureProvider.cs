using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.DisplayManagement.Theming;

/// <summary>
/// Provides Theming precompiled views when specific Layout and ViewStart files are seeked on the filesystem.
/// </summary>
public class ThemingViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
{
    public static readonly string ThemeLayoutFileName = "DefaultOrchardCoreThemingLayout" + RazorViewEngine.ViewExtension;

    public ThemingViewsFeatureProvider()
    {
    }

    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
    {
        feature.ViewDescriptors.Add(new CompiledViewDescriptor()
        {
            ExpirationTokens = Array.Empty<IChangeToken>(),
            RelativePath = "/_ViewStart" + RazorViewEngine.ViewExtension,
            Item = new TenantRazorCompiledItem(typeof(ThemeViewStart), "/_ViewStart")
        });

        feature.ViewDescriptors.Add(new CompiledViewDescriptor()
        {
            ExpirationTokens = Array.Empty<IChangeToken>(),
            RelativePath = '/' + ThemeLayoutFileName,
            Item = new TenantRazorCompiledItem(typeof(ThemeLayout), '/' + ThemeLayoutFileName)
        });
    }
}
