using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Diagnostics;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Modules.OrchardCore.Diagnostics;

public class DiagnosticsStartupFilterTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(0, false, true)]
    [InlineData(0, true, false)]
    [InlineData(0, true, true)]
    [InlineData(404, false, false)]
    [InlineData(404, false, true)]
    [InlineData(404, true, false)]
    [InlineData(404, true, true)]
    public async Task ErrorRenderingUsesAnIsolatedShellScope(int statusCode, bool asynchronous, bool renderingFails)
    {
        await UsingRequestScopeAsync(async (context, requestScope) =>
        {
            var requestServices = context.RequestServices;
            var requestTitle = requestServices.GetRequiredService<IPageTitleBuilder>();
            requestTitle.AddSegment(new HtmlString("Original"), "0");
            var requestResource = requestServices.GetRequiredService<ScopedResource>();
            var requestFeature = new RazorViewFeature { ThemeLayout = Mock.Of<IZoneHolding>() };
            var requestActionContext = new ActionContext();
            context.Features.Set(requestFeature);
            context.Items["OrchardCore:ActionContext"] = requestActionContext;
            ShellScope.Set("Original", true);

            var originalException = new InvalidOperationException("Original failure");
            var renderingException = new InvalidOperationException("Error rendering failure");
            ScopedResource errorResource = null;
            ShellScope errorScope = null;
            var beforeDisposeCalled = false;
            var errorRenderCount = 0;

            var pipeline = CreatePipeline(requestScope.ShellContext.ServiceProvider, async httpContext =>
            {
                if (!httpContext.Request.Path.StartsWithSegments("/Error"))
                {
                    if (statusCode == 0)
                    {
                        throw originalException;
                    }

                    httpContext.Response.StatusCode = statusCode;
                    return;
                }

                errorRenderCount++;
                errorScope = ShellScope.Current;
                Assert.NotSame(requestScope, errorScope);
                Assert.Same(requestScope.ShellContext, errorScope.ShellContext);
                Assert.Same(requestServices, httpContext.RequestServices);
                Assert.Null(ShellScope.Get("Original"));
                Assert.Null(httpContext.Features.Get<RazorViewFeature>());
                Assert.False(httpContext.Items.ContainsKey("OrchardCore:ActionContext"));
                Assert.Equal(httpContext.Features.Get<IExceptionHandlerFeature>() is not null ? "/Error" : "/Error/404",
                    httpContext.Request.Path.Value);

                var errorTitle = httpContext.RequestServices.GetRequiredService<IPageTitleBuilder>();
                Assert.NotSame(requestTitle, errorTitle);
                Assert.Same(errorTitle, ShellScope.Services.GetRequiredService<IPageTitleBuilder>());
                Assert.Same(errorTitle, new PageTitleShapes().Title);
                Assert.Same(errorTitle, new TestRazorPage { ViewContext = new ViewContext { HttpContext = httpContext } }.Title);
                Assert.Same(HtmlString.Empty, errorTitle.GenerateTitle());
                errorTitle.AddSegment(new HtmlString("Error"), "0");

                errorResource = httpContext.RequestServices.GetRequiredService<ScopedResource>();
                Assert.NotSame(requestResource, errorResource);
                Assert.Same(errorResource, ShellScope.Services.GetRequiredService<ScopedResource>());
                Assert.Equal(2, requestScope.ShellContext.ActiveScopes);

                ShellScope.RegisterBeforeDispose(scope =>
                {
                    Assert.Same(errorScope, scope);
                    Assert.Same(scope, ShellScope.Current);
                    Assert.Same(errorResource, httpContext.RequestServices.GetRequiredService<ScopedResource>());
                    beforeDisposeCalled = true;
                    return Task.CompletedTask;
                });

                httpContext.Features.Set(new RazorViewFeature());
                httpContext.Items["OrchardCore:ActionContext"] = new ActionContext();
                if (asynchronous)
                {
                    await Task.Yield();
                    Assert.Same(errorScope, ShellScope.Current);
                    Assert.Same(errorTitle, httpContext.RequestServices.GetRequiredService<IPageTitleBuilder>());
                }

                if (renderingFails)
                {
                    throw renderingException;
                }

                httpContext.Response.ContentType = "text/html";
            }, Environments.Production);

            var execution = pipeline(context);
            if (!asynchronous)
            {
                Assert.True(execution.IsCompleted);
            }

            if (renderingFails)
            {
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => execution);
                Assert.Same(statusCode == 0 ? originalException : renderingException, exception);
            }
            else
            {
                await execution;
                Assert.Equal(statusCode == 0 ? 500 : statusCode, context.Response.StatusCode);
            }

            Assert.Equal(statusCode != 0 && renderingFails ? 2 : 1, errorRenderCount);
            Assert.True(beforeDisposeCalled);
            Assert.True(errorResource.Disposed);
            Assert.False(requestResource.Disposed);
            Assert.Same(requestScope, ShellScope.Current);
            Assert.Same(requestServices, context.RequestServices);
            Assert.Same(requestTitle, context.RequestServices.GetRequiredService<IPageTitleBuilder>());
            Assert.Same(requestFeature, context.Features.Get<RazorViewFeature>());
            Assert.Same(requestActionContext, context.Items["OrchardCore:ActionContext"]);
            Assert.True(ShellScope.Get<bool>("Original"));
            Assert.Equal(1, requestScope.ShellContext.ActiveScopes);
            Assert.Equal("/original", context.Request.Path);
        }, asynchronous);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ErrorRenderingRestoresMissingCaches(bool renderingFails)
    {
        await UsingRequestScopeAsync(async (context, requestScope) =>
        {
            var exception = new InvalidOperationException();
            var pipeline = CreatePipeline(requestScope.ShellContext.ServiceProvider, httpContext =>
            {
                if (httpContext.Request.Path == "/original")
                {
                    httpContext.Response.StatusCode = 404;
                    return Task.CompletedTask;
                }

                httpContext.Features.Set(new RazorViewFeature());
                httpContext.Items["OrchardCore:ActionContext"] = new ActionContext();
                return renderingFails ? Task.FromException(exception) : Task.CompletedTask;
            }, Environments.Production);

            if (renderingFails)
            {
                Assert.Same(exception, await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline(context)));
            }
            else
            {
                await pipeline(context);
            }

            Assert.Null(context.Features.Get<RazorViewFeature>());
            Assert.False(context.Items.ContainsKey("OrchardCore:ActionContext"));
            Assert.Same(requestScope, ShellScope.Current);
        });
    }

    [Theory]
    [InlineData(200, "Production", "/original", false)]
    [InlineData(404, "Production", "/missing.css", false)]
    [InlineData(404, "Development", "/original", true)]
    public async Task OnlyErrorReExecutionCreatesAScope(int statusCode, string environment, string path, bool reExecute)
    {
        await UsingRequestScopeAsync(async (context, requestScope) =>
        {
            context.Request.Path = path;
            var calls = 0;
            var pipeline = CreatePipeline(requestScope.ShellContext.ServiceProvider, httpContext =>
            {
                calls++;
                Assert.Equal(calls == 1, ReferenceEquals(requestScope, ShellScope.Current));
                Assert.Equal(calls, requestScope.ShellContext.ActiveScopes);
                httpContext.Response.StatusCode = statusCode;
                return Task.CompletedTask;
            }, environment);

            var task = pipeline(context);
            Assert.True(task.IsCompletedSuccessfully);
            await task;
            Assert.Equal(reExecute ? 2 : 1, calls);
            Assert.Same(requestScope, ShellScope.Current);
        });
    }

    [Fact]
    public async Task DevelopmentDoesNotHandleExceptions()
    {
        await UsingRequestScopeAsync(async (context, requestScope) =>
        {
            var exception = new InvalidOperationException();
            var pipeline = CreatePipeline(requestScope.ShellContext.ServiceProvider,
                _ => Task.FromException(exception), Environments.Development);

            Assert.Same(exception, await Assert.ThrowsAsync<InvalidOperationException>(() => pipeline(context)));
            Assert.Same(requestScope, ShellScope.Current);
            Assert.Equal(1, requestScope.ShellContext.ActiveScopes);
        });
    }

    private static RequestDelegate CreatePipeline(IServiceProvider services, RequestDelegate endpoint, string environment)
    {
        var builder = new ApplicationBuilder(services);
        var filter = new DiagnosticsStartupFilter(
            Mock.Of<IHostEnvironment>(host => host.EnvironmentName == environment));
        filter.Configure(app => app.Run(endpoint))(builder);
        return builder.Build();
    }

    private static async Task UsingRequestScopeAsync(Func<HttpContext, ShellScope, Task> execute, bool asynchronous = false)
    {
        await using var services = new ServiceCollection()
            .AddLogging()
            .AddMetrics()
            .AddSingleton(new DiagnosticListener("Microsoft.AspNetCore"))
            .AddScoped<ViewContextAccessor>()
            .AddScoped<IPageTitleBuilder, PageTitleBuilder>()
            .AddScoped(_ => new ScopedResource(asynchronous))
            .BuildServiceProvider(validateScopes: true);

        var shellContext = new ShellContext
        {
            Settings = new ShellSettings().AsDefaultShell().AsRunning(),
            ServiceProvider = services,
            IsActivated = true,
        };

        await new ShellScope(shellContext).UsingAsync(scope =>
        {
            var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
            context.UseShellScopeServices();
            context.Request.Path = "/original";
            return execute(context, scope);
        });
    }

    private sealed class TestRazorPage : RazorPage<object>
    {
        public override Task ExecuteAsync() => Task.CompletedTask;
    }

    private sealed class ScopedResource(bool asynchronous) : IAsyncDisposable
    {
        public bool Disposed { get; private set; }

        public ValueTask DisposeAsync()
        {
            if (asynchronous)
            {
                return Awaited();
            }

            Disposed = true;
            return ValueTask.CompletedTask;

            async ValueTask Awaited()
            {
                await Task.Yield();
                Disposed = true;
            }
        }
    }
}
