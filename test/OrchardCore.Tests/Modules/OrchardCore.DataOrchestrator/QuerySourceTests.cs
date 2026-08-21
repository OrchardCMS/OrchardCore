using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.DataOrchestrator.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.DataOrchestrator;

public class QuerySourceTests
{
    [Fact]
    public async Task ExecuteAsync_WithNoQueryName_Fails()
    {
        var source = new QuerySource();
        var context = new EtlExecutionContext(
            new EtlPipelineDefinition(),
            Mock.Of<IEtlActivityLibrary>(),
            new ServiceCollection().BuildServiceProvider(),
            NullLogger.Instance,
            TestContext.Current.CancellationToken);

        var result = await source.ExecuteAsync(context);

        Assert.False(result.IsSuccess);
        Assert.Contains("query", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Null(context.DataStream);
    }
}
