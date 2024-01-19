using System;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace OrchardCore.Mvc
{
    public class RazorCompilationFileProviderAccessor
    {
        private readonly MvcRazorRuntimeCompilationOptions _options;
        private IFileProvider _compositeFileProvider;

        public RazorCompilationFileProviderAccessor(IOptions<MvcRazorRuntimeCompilationOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options.Value;
        }

        public IFileProvider FileProvider
        {
            get
            {
                _compositeFileProvider ??= GetCompositeFileProvider(_options);

                return _compositeFileProvider;
            }
        }

        private static IFileProvider GetCompositeFileProvider(MvcRazorRuntimeCompilationOptions options)
        {
            var fileProviders = options.FileProviders;

            if (fileProviders.Count == 1)
            {
                return fileProviders[0];
            }

            return new CompositeFileProvider(fileProviders);
        }
    }
}
