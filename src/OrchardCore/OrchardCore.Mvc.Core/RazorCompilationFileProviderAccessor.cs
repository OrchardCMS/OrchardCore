using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace OrchardCore.Mvc;

// Note: MvcRazorRuntimeCompilationOptions is deprecated in .NET 10
// This class is kept for backward compatibility but will be removed in future versions
#pragma warning disable ASPDEPR003 // Razor runtime compilation is obsolete
public class RazorCompilationFileProviderAccessor
{
    private readonly Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.MvcRazorRuntimeCompilationOptions _options;
    private IFileProvider _compositeFileProvider;

    public RazorCompilationFileProviderAccessor(IOptions<Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.MvcRazorRuntimeCompilationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

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

    private static IFileProvider GetCompositeFileProvider(Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.MvcRazorRuntimeCompilationOptions options)
    {
        var fileProviders = options.FileProviders;

        if (fileProviders.Count == 1)
        {
            return fileProviders[0];
        }

        return new CompositeFileProvider(fileProviders);
    }
}
#pragma warning restore ASPDEPR003
