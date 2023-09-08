using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.Razor;

namespace OrchardCore.DisplayManagement.Theming
{
    /// <summary>
    /// Provides Theming precompiled views when specific Layout and ViewStart files are seeked on the filesystem.
    /// </summary>
    public class ThemingViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        public static readonly string ThemeLayoutFileName = $"DefaultOrchardCoreThemingLayout{RazorViewEngine.ViewExtension}";

        private static readonly string _themeLayoutFilePath = $"/{ThemeLayoutFileName}";
        private static readonly string _viewStart = $"{_viewStartWithoutExtension}{RazorViewEngine.ViewExtension}";
        private static readonly string _viewStartWithoutExtension = "/_ViewStart";

        public ThemingViewsFeatureProvider()
        {
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            feature.ViewDescriptors.Add(new CompiledViewDescriptor()
            {
                ExpirationTokens = Array.Empty<IChangeToken>(),
                RelativePath = _viewStart,
                Item = new RazorViewCompiledItem(
                    typeof(ThemeViewStart),
                    MvcViewDocumentClassifierPass.MvcViewDocumentKind,
                    _viewStartWithoutExtension),
            });

            feature.ViewDescriptors.Add(new CompiledViewDescriptor()
            {
                ExpirationTokens = Array.Empty<IChangeToken>(),
                RelativePath = _themeLayoutFilePath,
                Item = new RazorViewCompiledItem(
                    typeof(ThemeLayout),
                    MvcViewDocumentClassifierPass.MvcViewDocumentKind,
                    _themeLayoutFilePath),
            });
        }
    }
}
