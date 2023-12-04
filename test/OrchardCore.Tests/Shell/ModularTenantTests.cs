using OrchardCore.DisplayManagement.FileProviders;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Shell;

public class ModularTenantTests
{
    [Fact]
    public async Task ConfigureIStartupServicesBeforeRoutes()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IHostEnvironment>(new StubHostingEnvironment())
            .AddSingleton<IWebHostEnvironment, FakeWebHostEnvironment>();

        services.AddOrchardCore();
        services.AddSingleton<IStartupFilter, TestStartupFilter>();
        services.AddSingleton<IDistributedLock, LocalLock>()
            .AddLogging()
            ;

        var shellSettings = new ShellSettings().AsDefaultShell().AsRunning();

        var shellContext = new ShellContext()
        {
            Settings = shellSettings,
            ServiceProvider = services.BuildServiceProvider(),
        };

        await (await shellContext.CreateScopeAsync()).UsingAsync(scope =>
        {
            try
            {
                var builder = new ApplicationBuilder(scope.ShellContext.ServiceProvider);

                builder.UseOrchardCore();

                var app = builder.Build();

                var httpContext = new DefaultHttpContext
                {
                    RequestServices = new ShellScopeServices(scope.ShellContext.ServiceProvider)
                };

                app.Invoke(httpContext);
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }

            return Task.CompletedTask;
        });
    }

    internal sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Benchmark";

        public IFileProvider ContentRootFileProvider { get; set; } = new FakeFileProvider();

        public string ContentRootPath { get; set; }

        public string EnvironmentName { get; set; }

        public IFileProvider WebRootFileProvider { get; set; } = new FakeFileProvider();

        public string WebRootPath { get; set; }
    }

    internal class FakeFileProvider : IFileProvider
    {
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return new NotFoundDirectoryContents();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return new ContentFileInfo("name", "content");
        }

        public IChangeToken Watch(string filter)
        {
            return new CancellationChangeToken(CancellationToken.None);
        }
    }

    private class TestStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                if (app.Properties.TryGetValue(ShellPipelineExtensions.EndpointRouteBuilder, out var obj))
                {
                    throw new InvalidOperationException($"{nameof(IStartupFilter.Configure)} must be in execution pipeline before {nameof(EndpointRoutingApplicationBuilderExtensions.UseRouting)} to 'Configure(...)' in the application startup code.");
                }

                next(app);
            };
        }
    }
}
