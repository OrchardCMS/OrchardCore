using OrchardCore.Workflows.Models;

namespace OrchardCore.Tests.Workflows;

public class WorkflowExecutionDataTests
{
    [Theory]
    [InlineData("😀")]
    [InlineData("e\u0301")]
    [InlineData("🇺🇸")]
    [InlineData("👩‍💻")]
    public void Truncate_InsideTextElement_KeepsWholeElements(string element)
    {
        var prefix = new string('a', WorkflowExecutionData.MaxValueLength - 1);

        var result = WorkflowExecutionData.Truncate(prefix + element + "more", out var truncated);

        Assert.True(truncated);
        Assert.Equal(prefix + WorkflowExecutionData.Ellipsis, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(WorkflowExecutionData.MaxValueLength)]
    public void Truncate_WithinLimit_ReturnsUnchangedText(int length)
    {
        var text = new string('a', length);

        Assert.Equal(text, WorkflowExecutionData.Truncate(text, out var truncated));
        Assert.False(truncated);
    }
}
