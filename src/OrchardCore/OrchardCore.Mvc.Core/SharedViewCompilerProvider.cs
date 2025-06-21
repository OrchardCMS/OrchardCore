using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Mvc;

/// <summary>
/// Shares across tenants the same <see cref="IViewCompiler"/>.
/// </summary>
public class SharedViewCompilerProvider : IViewCompilerProvider
{
    private readonly object _synLock = new();
    private static IViewCompiler _compiler;
    private readonly IServiceProvider _services;

    public SharedViewCompilerProvider(IServiceProvider services)
    {
        _services = services;
    }

    public IViewCompiler GetCompiler()
    {
        if (_compiler is not null)
        {
            return _compiler;
        }

        lock (_synLock)
        {
            if (_compiler is not null)
            {
                return _compiler;
            }

            _compiler = _services
                .GetServices<IViewCompilerProvider>()
                .FirstOrDefault()
                ?.GetCompiler();
        }

        return _compiler;
    }
}
