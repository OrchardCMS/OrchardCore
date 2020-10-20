using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Autoroute.Services;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Data.Documents;
using OrchardCore.Documents;
using OrchardCore.Documents.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;
using Xunit;

namespace OrchardCore.Tests.Routing
{
    public class AutorouteEntriesTests
    {
        [Fact]
        public async Task ShouldGetContainedEntryByPath()
        {
            // Setup
            var shellContext = CreateShellContext();

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Act
                var initialEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path", "contained")
                };

                await entries.AddEntriesAsync(initialEntries);
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Test
                (var result, var containedEntry) = await entries.TryGetEntryByPathAsync("/contained-path");

                Assert.True(result);
                Assert.Equal("contained", containedEntry.ContainedContentItemId);
            });
        }

        [Fact]
        public async Task ShouldGetEntryByContainedContentItemId()
        {
            // Setup
            var shellContext = CreateShellContext();

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Act
                var initialEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path", "contained")
                };

                await entries.AddEntriesAsync(initialEntries);
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Test
                (var result, var containedEntry) = await entries.TryGetEntryByContentItemIdAsync("contained");

                Assert.True(result);
                Assert.Equal("/contained-path", containedEntry.Path);
            });
        }

        [Fact]
        public async Task RemovesContainedEntriesWhenContainerRemoved()
        {
            // Setup
            var shellContext = CreateShellContext();

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Act
                var initialEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path", "contained")
                };

                await entries.AddEntriesAsync(initialEntries);

                await entries.RemoveEntriesAsync(new[] { new AutorouteEntry("container", "container-path", null, null) });
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Test
                (var result, var containedEntry) = await entries.TryGetEntryByPathAsync("/contained-path");

                Assert.False(result);
            });
        }

        [Fact]
        public async Task RemovesContainedEntriesWhenDeleted()
        {
            // Setup
            var shellContext = CreateShellContext();

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Act
                var initialEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path1", "contained1"),
                    new AutorouteEntry("container", "contained-path2", "contained2")
                };

                await entries.AddEntriesAsync(initialEntries);

                var updatedEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path1", "contained1")
                };

                await entries.AddEntriesAsync(updatedEntries);
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Test
                (var result, var containedEntry) = await entries.TryGetEntryByPathAsync("/contained-path2");

                Assert.False(result);
            });
        }

        [Fact]
        public async Task RemovesOldContainedPaths()
        {
            // Setup
            var shellContext = CreateShellContext();

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Act
                var initialEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path-old", "contained")
                };

                await entries.AddEntriesAsync(initialEntries);

                var updatedEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path-new", "contained")
                };

                await entries.AddEntriesAsync(updatedEntries);
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Test
                (var result, var containedEntry) = await entries.TryGetEntryByPathAsync("/contained-path-old");

                Assert.False(result);
            });
        }

        [Fact]
        public async Task RemovesOldPaths()
        {
            // Setup
            var shellContext = CreateShellContext();

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Act
                await entries.AddEntriesAsync(new[] { new AutorouteEntry("container", "container-path", null, null) });

                await entries.RemoveEntriesAsync(new[] { new AutorouteEntry("container", "container-path", null, null) });
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IAutorouteEntries>();

                // Test
                (var result, var containedEntry) = await entries.TryGetEntryByPathAsync("/container-path");

                Assert.False(result);
            });
        }

        private ShellContext CreateShellContext()
        {
            return new ShellContext()
            {
                Settings = new ShellSettings() { Name = ShellHelper.DefaultShellName, State = TenantState.Running },
                ServiceProvider = CreateServiceProvider()
            };
        }

        private IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IShellConfiguration>(sp => new ShellConfiguration());
            services.AddSingleton<IDistributedCache>(sp => new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())));
            services.AddSingleton<IMemoryCache>(sp => new MemoryCache(Options.Create(new MemoryCacheOptions())));

            services.AddScoped<IDocumentStore, StubDocumentstore>();
            services.AddScoped(typeof(IDocumentManager<>), typeof(DocumentManager<>));
            services.AddScoped(typeof(IVolatileDocumentManager<>), typeof(VolatileDocumentManager<>));
            services.AddSingleton<IDocumentOptionsFactory, DocumentOptionsFactory>();
            services.AddTransient(typeof(DocumentOptions<>));

            services.AddSingleton<IAutorouteEntries, StubAutorouteEntries>();

            return services.BuildServiceProvider();
        }

        private class StubAutorouteEntries : AutorouteEntries
        {
            protected override Task<AutorouteDocument> CreateDocumentAsync() => Task.FromResult(new AutorouteDocument());
        }

        private class StubDocumentstore : IDocumentStore
        {
            public Task<T> GetOrCreateMutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new() => throw new NotImplementedException();
            public Task<(bool, T)> GetOrCreateImmutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new() => throw new NotImplementedException();
            public Task UpdateAsync<T>(T document, Func<T, Task> updateCache, bool checkConcurrency = false) => throw new NotImplementedException();

            public void Cancel() => throw new NotImplementedException();
            public void AfterCommitFailure<T>(DocumentStoreCommitFailureDelegate afterCommit) => throw new NotImplementedException();

            public void AfterCommitSuccess<T>(DocumentStoreCommitSuccessDelegate afterCommit)
                => ShellScope.RegisterBeforeDispose(scope => afterCommit());

            public Task CommitAsync() => throw new NotImplementedException();
        }
    }
}
