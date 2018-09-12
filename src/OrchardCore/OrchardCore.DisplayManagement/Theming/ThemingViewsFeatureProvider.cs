using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.DisplayManagement.Theming
{
    /// <summary>
    /// Provides Theming precompiled views when specific Layout and ViewStart files are seeked on the filesystem.
    /// </summary>
    public class ThemingViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        public static string ThemeLayoutFileName = "DefaultOrchardCoreThemingLayout" + RazorViewEngine.ViewExtension;

        private readonly IHostingEnvironment _hostingEnvironment;

        public ThemingViewsFeatureProvider(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            if (!parts.Where(p => p.Name == _hostingEnvironment.ApplicationName).Any())
            {
                return;
            }

            feature.ViewDescriptors.Add(new CompiledViewDescriptor()
            {
                ExpirationTokens = Array.Empty<IChangeToken>(),
                RelativePath = ViewPath.NormalizePath("/_ViewStart" + RazorViewEngine.ViewExtension),
                ViewAttribute = new RazorViewAttribute("/_ViewStart" + RazorViewEngine.ViewExtension, typeof(ThemeViewStart)),
                IsPrecompiled = true,
            });

            feature.ViewDescriptors.Add(new CompiledViewDescriptor()
            {
                ExpirationTokens = Array.Empty<IChangeToken>(),
                RelativePath = ViewPath.NormalizePath(ThemeLayoutFileName),
                ViewAttribute = new RazorViewAttribute(ThemeLayoutFileName, typeof(ThemeLayout)),
                IsPrecompiled = true,
            });
        }
    }
}