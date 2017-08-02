using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.Extensions.Primitives;

namespace Orchard.DisplayManagement.Theming
{
    public class ThemingViewsFeatureProvider : IApplicationFeatureProvider<ViewsFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ViewsFeature feature)
        {
            feature.ViewDescriptors.Add(new CompiledViewDescriptor()
            {
                ExpirationTokens = Array.Empty<IChangeToken>(),
                RelativePath = ViewPath.NormalizePath("/_ViewStart.cshtml"),
                ViewAttribute = new RazorViewAttribute("/_ViewStart.cshtml", typeof(ThemeViewStart)),
                IsPrecompiled = true,
            });

            feature.ViewDescriptors.Add(new CompiledViewDescriptor()
            {
                ExpirationTokens = Array.Empty<IChangeToken>(),
                RelativePath = ViewPath.NormalizePath("/Views/Shared/_Layout.cshtml"),
                ViewAttribute = new RazorViewAttribute("/Views/Shared/_Layout.cshtml", typeof(ThemeLayout)),
                IsPrecompiled = true,
            });
        }
    }
}