using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using OrchardCore.Modules;

namespace OrchardCore.Mvc;

/// <summary>
/// Provides a composite <see cref="IFileProvider"/> that combines the application's view file
/// provider with the content root file provider. Used by view location expanders to discover
/// which modules have shared views.
/// </summary>
public class ViewFileProviderAccessor
{
    public ViewFileProviderAccessor(
        IHostEnvironment hostingEnvironment,
        IApplicationContext applicationContext)
    {
        FileProvider = new CompositeFileProvider(
            new ApplicationViewFileProvider(applicationContext),
            hostingEnvironment.ContentRootFileProvider);
    }

    public IFileProvider FileProvider { get; }
}
