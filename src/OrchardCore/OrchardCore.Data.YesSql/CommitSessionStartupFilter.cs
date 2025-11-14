using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace OrchardCore.Data;

/// <summary>
/// Startup filter that registers the CommitSessionMiddleware late in the pipeline.
/// </summary>
public class CommitSessionStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            builder.UseMiddleware<CommitSessionMiddleware>();
            next(builder);
        };
    }
}
