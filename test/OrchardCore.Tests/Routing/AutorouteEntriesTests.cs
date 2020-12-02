using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Services;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
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

            await shellContext.CreateScope().UsingAsync(scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

                // Act
                var initialEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path", "contained")
                };

                entries.AddEntries(initialEntries);

                return Task.CompletedTask;
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

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

            await shellContext.CreateScope().UsingAsync(scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

                // Act
                var initialEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path", "contained")
                };

                entries.AddEntries(initialEntries);

                return Task.CompletedTask;
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

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

            await shellContext.CreateScope().UsingAsync(scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

                // Act
                var initialEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path", "contained")
                };

                entries.AddEntries(initialEntries);

                entries.RemoveEntries(new[] { new AutorouteEntry("container", "container-path", null, null) });

                return Task.CompletedTask;
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

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

            await shellContext.CreateScope().UsingAsync(scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

                // Act
                var initialEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path1", "contained1"),
                    new AutorouteEntry("container", "contained-path2", "contained2")
                };

                entries.AddEntries(initialEntries);

                var updatedEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path1", "contained1")
                };

                entries.AddEntries(updatedEntries);

                return Task.CompletedTask;
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

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

            await shellContext.CreateScope().UsingAsync(scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

                // Act
                var initialEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path-old", "contained")
                };

                entries.AddEntries(initialEntries);

                var updatedEntries = new List<AutorouteEntry>()
                {
                    new AutorouteEntry("container", "container-path"),
                    new AutorouteEntry("container", "contained-path-new", "contained")
                };

                entries.AddEntries(updatedEntries);

                return Task.CompletedTask;
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

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

            await shellContext.CreateScope().UsingAsync(scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

                // Act
                entries.AddEntries(new[] { new AutorouteEntry("container", "container-path", null, null) });

                entries.RemoveEntries(new[] { new AutorouteEntry("container", "container-path", null, null) });

                return Task.CompletedTask;
            });

            await shellContext.CreateScope().UsingAsync(async scope =>
            {
                var entries = scope.ServiceProvider.GetRequiredService<IStubAutorouteEntries>();

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
            services.AddSingleton<IStubAutorouteEntries, StubAutorouteEntries>();
            return services.BuildServiceProvider();
        }

        public interface IStubAutorouteEntries : IAutorouteEntries
        {
            void AddEntries(IEnumerable<AutorouteEntry> entries);
            void RemoveEntries(IEnumerable<AutorouteEntry> entries);
        }

        private class StubAutorouteEntries : AutorouteEntries, IStubAutorouteEntries
        {
            public new void AddEntries(IEnumerable<AutorouteEntry> entries) => base.AddEntries(entries);
            public new void RemoveEntries(IEnumerable<AutorouteEntry> entries) => base.RemoveEntries(entries);
            protected override Task InitializeEntriesAsync() => Task.CompletedTask;
        }
    }
}
