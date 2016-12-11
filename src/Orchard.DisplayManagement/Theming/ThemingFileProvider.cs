using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Orchard.DisplayManagement.Theming
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// necessary to override the default Layout system from Razor with a Shape based one.
    /// </summary>
    public class ThemingFileProvider : IFileProvider
    {
        private readonly ContentFileInfo _viewStartFileInfo;
        private readonly ContentFileInfo _viewImportsFileInfo;
        private readonly ContentFileInfo _layoutFileInfo;

        public ThemingFileProvider()
        {
            _viewStartFileInfo = new ContentFileInfo("_ViewStart.cshtml", "@{ Layout = \"~/Views/Shared/_Layout.cshtml\"; }");
            _viewImportsFileInfo = new ContentFileInfo("_ViewImports.cshtml", "@inherits Orchard.DisplayManagement.Razor.RazorPage<TModel>");
            _layoutFileInfo = new ContentFileInfo("_Layout.cshtml", @"﻿@{var body = RenderLayoutBody();ThemeLayout.Content.Add(body);}@await DisplayAsync(ThemeLayout)");
        }
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return null;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == "/_ViewStart.cshtml")
            {
                return _viewStartFileInfo;
            }
            else if (subpath == "/Views/_ViewImports.cshtml")
            {
                return _viewImportsFileInfo;
            }
            else if (subpath == "/Views/Shared/_Layout.cshtml")
            {
                return _layoutFileInfo;
            }

            return null;
        }

        public IChangeToken Watch(string filter)
        {
            return null;
        }
    }
}
