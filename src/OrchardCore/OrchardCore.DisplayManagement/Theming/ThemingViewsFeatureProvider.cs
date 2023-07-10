using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.Razor;

namespace OrchardCore.DisplayManagement.Theming
{
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
                Item = new RazorViewCompiledItem(typeof(ThemeViewStart), @"mvc.1.0.view", "/_ViewStart")
            });

            feature.ViewDescriptors.Add(new CompiledViewDescriptor()
            {
                ExpirationTokens = Array.Empty<IChangeToken>(),
                RelativePath = '/' + ThemeLayoutFileName,
                Item = new RazorViewCompiledItem(typeof(ThemeLayout), @"mvc.1.0.view", '/' + ThemeLayoutFileName)
            });
        }
    }
}
