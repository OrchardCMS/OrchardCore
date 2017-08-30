using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Orchard.Mvc
{
    public class ModularPageActionDescriptorChangeProvider : IActionDescriptorChangeProvider
    {
        private const string PageSearchPattern = "/Pages/**/*.cshtml";

        private readonly IFileProvider _fileProvider;
        private readonly string _searchPattern;

        public ModularPageActionDescriptorChangeProvider(
            RazorTemplateEngine templateEngine,
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            IOptions<RazorPagesOptions> razorPagesOptions)
        {
            _fileProvider = fileProviderAccessor.FileProvider;
            var rootDirectory = razorPagesOptions.Value.RootDirectory.TrimEnd('/');
            _searchPattern = rootDirectory + "/**" + PageSearchPattern;
        }

        public IChangeToken GetChangeToken()
        {
            return _fileProvider.Watch(_searchPattern);
        }
    }
}