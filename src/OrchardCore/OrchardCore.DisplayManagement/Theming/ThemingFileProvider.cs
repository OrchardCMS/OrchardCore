using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.FileProviders;

namespace OrchardCore.DisplayManagement.Theming
{
    /// <summary>
    /// This custom <see cref="IFileProvider"/> implementation provides the file contents
    /// necessary to override the default Layout system from Razor with a Shape based one.
    /// </summary>
    public class ThemingFileProvider : IFileProvider
    {
        private readonly ContentFileInfo _viewImportsFileInfo;

        public ThemingFileProvider()
        {
            _viewImportsFileInfo = new ContentFileInfo("_ViewImports" + RazorViewEngine.ViewExtension, "@inherits OrchardCore.DisplayManagement.Razor.RazorPage<TModel>");
        }
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return NotFoundDirectoryContents.Singleton;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == "/_ViewImports" + RazorViewEngine.ViewExtension)
            {
                return _viewImportsFileInfo;
            }

            return new NotFoundFileInfo(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return NullChangeToken.Singleton;
        }
    }
}
