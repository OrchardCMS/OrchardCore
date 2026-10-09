using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Modules;

public class ShellRequestPipeline : IShellPipeline
{
    public RequestDelegate Next { get; set; } = null!;
    public Task Invoke(object context) => Next((HttpContext)context);
}
