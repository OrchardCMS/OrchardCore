using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Orchard.Mvc.RazorPages
{
    public class ModularPageActionDescriptorChangeProvider : IActionDescriptorChangeProvider
    {
        private readonly IFileProvider _fileProvider;
        private readonly string _searchPattern;

        public ModularPageActionDescriptorChangeProvider(
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            IOptions<RazorPagesOptions> razorPagesOptions)
        {
            _fileProvider = fileProviderAccessor.FileProvider;
            _searchPattern = razorPagesOptions.Value.RootDirectory.TrimEnd('/')
                + "/**/Pages/**/*" + RazorViewEngine.ViewExtension;
        }

        public IChangeToken GetChangeToken()
        {
            return _fileProvider.Watch(_searchPattern);
        }
    }
}