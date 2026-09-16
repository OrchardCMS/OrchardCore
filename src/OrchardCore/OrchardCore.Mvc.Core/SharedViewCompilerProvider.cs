using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Mvc;

/// <summary>
/// Shares across tenants the same <see cref="IViewCompiler"/>.
/// </summary>
public class SharedViewCompilerProvider : IViewCompilerProvider
{
    private readonly object _synLock = new();
    private static IViewCompiler s_compiler;
    private readonly IServiceProvider _services;

    public SharedViewCompilerProvider(IServiceProvider services)
    {
        _services = services;
    }

    public IViewCompiler GetCompiler()
    {
        if (s_compiler is not null)
        {
            return s_compiler;
        }

        lock (_synLock)
        {
            if (s_compiler is not null)
            {
                return s_compiler;
            }

            s_compiler = _services
                .GetServices<IViewCompilerProvider>()
                .FirstOrDefault()
                ?.GetCompiler();
        }

        return s_compiler;
    }
}
