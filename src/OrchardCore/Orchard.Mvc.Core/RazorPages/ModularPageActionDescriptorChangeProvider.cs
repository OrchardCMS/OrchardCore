using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Orchard.Mvc.RazorPages
{
    public class ModularPageActionDescriptorChangeProvider : IActionDescriptorChangeProvider
    {
        private const string PageFolder = "/Pages";
        private const string PageSearchPattern = PageFolder + "/**/*.cshtml";
        private const string ModularPageSearchPattern = "/**" + PageSearchPattern;

        private readonly IFileProvider _fileProvider;
        private readonly string _searchPattern;

        public ModularPageActionDescriptorChangeProvider(
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            IOptions<RazorPagesOptions> razorPagesOptions)
        {
            _fileProvider = fileProviderAccessor.FileProvider;
            _searchPattern = razorPagesOptions.Value.RootDirectory.TrimEnd('/') + ModularPageSearchPattern;
        }

        public IChangeToken GetChangeToken()
        {
            return _fileProvider.Watch(_searchPattern);
        }
    }
}