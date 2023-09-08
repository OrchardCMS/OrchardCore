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
        public static readonly string ThemeLayoutFilePath = $"/{ThemeLayoutFileName}";
        public static readonly string MvcViewDocumentKind = $@"""{MvcViewDocumentClassifierPass.MvcViewDocumentKind}""";

        private static readonly string _viewStartPath = "/_ViewStart";
        private static readonly string _viewStartPathWithExtension = $"{_viewStartPath}{RazorViewEngine.ViewExtension}";

        public ThemingViewsFeatureProvider()
        {
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            feature.ViewDescriptors.Add(new CompiledViewDescriptor()
            {
                ExpirationTokens = Array.Empty<IChangeToken>(),
                RelativePath = _viewStartPathWithExtension,
                Item = new RazorViewCompiledItem(
                    typeof(ThemeViewStart),
                    MvcViewDocumentKind,
                    _viewStartPath),
            });

            feature.ViewDescriptors.Add(new CompiledViewDescriptor()
            {
                ExpirationTokens = Array.Empty<IChangeToken>(),
                RelativePath = ThemeLayoutFilePath,
                Item = new RazorViewCompiledItem(
                    typeof(ThemeLayout),
                    MvcViewDocumentKind,
                    ThemeLayoutFilePath),
            });
        }
    }
}
