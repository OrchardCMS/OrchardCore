using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Mvc;

// Note: MvcRazorRuntimeCompilationOptions is deprecated in .NET 10
// This class is kept for backward compatibility but will be removed in future versions
#pragma warning disable ASPDEPR003 // Razor runtime compilation is obsolete
public sealed class RazorCompilationOptionsSetup : IConfigureOptions<Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.MvcRazorRuntimeCompilationOptions>
{
    private readonly IHostEnvironment _hostingEnvironment;
    private readonly IApplicationContext _applicationContext;

    public RazorCompilationOptionsSetup(IHostEnvironment hostingEnvironment, IApplicationContext applicationContext)
    {
        _hostingEnvironment = hostingEnvironment;
        _applicationContext = applicationContext;
    }

    public void Configure(Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.MvcRazorRuntimeCompilationOptions options)
    {
        // In dev mode or if there is no 'refs' folder, we don't register razor runtime compilation services.
        var refsFolderExists = Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "refs"));

        // But in some view location expanders we always use the file providers that we enlist here, so
        // we still need to add 'ContentRootFileProvider' that includes our 'ModuleEmbeddedFileProvider'.
        if (!_hostingEnvironment.IsDevelopment() || !refsFolderExists)
        {
            options.FileProviders.Insert(0, _hostingEnvironment.ContentRootFileProvider);
        }

        // To let the application behave as a module, its razor files are requested under the virtual
        // "Areas" folder, but they are still served from the file system by this custom provider.
        options.FileProviders.Insert(0, new ApplicationViewFileProvider(_applicationContext));

        if (_hostingEnvironment.IsDevelopment())
        {
            // While in development, razor files are 1st served from their module project locations.
            options.FileProviders.Insert(0, new ModuleProjectRazorFileProvider(_applicationContext));
        }
    }
}
#pragma warning restore ASPDEPR003
