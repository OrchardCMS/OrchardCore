using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Builders;

/// <summary>
/// An <see cref="IShellPipeline"/> that manages the <see cref="ShellContext.RequestsCount"/>.
/// </summary>
public class ShellPipeline : IShellPipeline
{
    private readonly ShellContext _shellContext;
    private readonly IShellPipeline _pipeline;

    /// <summary>
    /// Builds an <see cref="IShellPipeline"/> that manages the <see cref="ShellContext.RequestsCount"/>.
    /// </summary>
    public ShellPipeline(ShellContext shellcontext, IShellPipeline pipeline)
    {
        _shellContext = shellcontext;
        _pipeline = pipeline;
    }

    /// <summary>
    /// Manages the <see cref="ShellContext.RequestsCount"/> before executing the shell pipeline.
    /// </summary>
    public Task Invoke(object context)
    {
        Interlocked.Increment(ref _shellContext._requestsCount);
        return _pipeline.Invoke(context);
    }
}
