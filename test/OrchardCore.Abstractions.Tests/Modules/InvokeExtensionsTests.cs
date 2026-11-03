using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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

    [Fact]
    public async Task InvokeAsync_ArrayWithFailingHandlers_InvokesAllHandlersAndLogsEachFailureOnce()
    {
        // Arrange
        var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var invoked = new List<int>();
        var logger = new ErrorCountingLogger();

        // Act. The 2nd handler faults asynchronously, the 3rd one throws synchronously.
        var task = new[] { 1, 2, 3, 4 }.InvokeAsync(sink =>
        {
            invoked.Add(sink);

            return sink switch
            {
                2 => completionSource.Task,
                3 => throw new InvalidOperationException("Failed synchronously"),
                _ => Task.CompletedTask,
            };
        }, logger);

        completionSource.SetException(new InvalidOperationException("Failed asynchronously"));
        await task;

        // Assert
        Assert.Equal([1, 2, 3, 4], invoked);
        Assert.Equal(2, logger.Errors);
    }

    [Fact]
    public async Task InvokeAsync_ListWithAsyncHandler_InvokesAllHandlersSequentially()
    {
        // Arrange
        var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var invoked = new List<int>();

        // Act
        var task = new List<int> { 1, 2, 3 }.InvokeAsync(sink =>
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
    public void Invoke_ArrayWithFailingHandler_InvokesAllHandlers()
    {
        // Arrange
        var invoked = new List<int>();
        var logger = new ErrorCountingLogger();

        // Act
        new[] { 1, 2, 3 }.Invoke((sink, list) =>
        {
            list.Add(sink);

            if (sink == 2)
            {
                throw new InvalidOperationException("Failed");
            }
        }, invoked, logger);

        // Assert
        Assert.Equal([1, 2, 3], invoked);
        Assert.Equal(1, logger.Errors);
    }

    private sealed class ErrorCountingLogger : ILogger
    {
        public int Errors { get; private set; }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Error)
            {
                Errors++;
            }
        }
    }
}
