using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Builders;

public class ShellPipeline : IShellPipeline
{
    private readonly ShellContext _shellContext;
    private readonly IShellPipeline _pipeline;

    public ShellPipeline(ShellContext shellcontext, IShellPipeline pipeline)
    {
        _shellContext = shellcontext;
        _pipeline = pipeline;
    }

    public Task Invoke(object context)
    {
        Interlocked.Increment(ref _shellContext._requestsCount);
        return _pipeline.Invoke(context);
    }
}
