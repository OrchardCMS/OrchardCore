using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace OrchardCore.Modules;

public class InvokeExtensionsTests
{
    [Fact]
    public void InvokeAsync_CallbacksCompleteSynchronously_ReturnsCompletedTask()
    {
        // Arrange
        var invoked = new List<int>();

        // Act
        var task = new[] { 1, 2, 3 }.InvokeAsync(sink =>
        {
            invoked.Add(sink);
            return Task.CompletedTask;
        }, NullLogger.Instance);

        // Assert
        Assert.Same(Task.CompletedTask, task);
        Assert.Equal([1, 2, 3], invoked);
    }

    [Fact]
    public async Task InvokeAsync_AwaitIsRequired_ContinueSequentially()
    {
        // Arrange
        var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var invoked = new List<int>();

        // Act
        var task = new[] { 1, 2, 3 }.InvokeAsync(sink =>
        {
            invoked.Add(sink);
            return sink == 2 ? completionSource.Task : Task.CompletedTask;
        }, NullLogger.Instance);

        // Assert
        Assert.Equal([1, 2], invoked);
        Assert.False(task.IsCompleted);

        completionSource.SetResult();
        await task;

        Assert.Equal([1, 2, 3], invoked);
    }

    [Fact]
    public async Task InvokeAsync_CollectRemainingResults_SkipsNonFatalExceptions()
    {
        // Arrange
        // Act
        var results = await new[] { 1, 2, 3 }.InvokeAsync(sink => sink switch
        {
            1 => Task.FromResult(10),
            2 => Task.FromException<int>(new InvalidOperationException("Failed")),
            _ => Task.FromResult(30),
        }, NullLogger.Instance);

        // Assert
        Assert.Equal([10, 30], results.ToArray());
    }
}
