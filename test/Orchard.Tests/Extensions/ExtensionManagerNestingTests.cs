using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Manifests;
using Orchard.Environment.Shell;
using Orchard.Tests.Stubs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Orchard.Tests.Extensions
{
    public class ExtensionManagerNestingTests
    {
        private static IHostingEnvironment HostingEnvrionment
            = new StubHostingEnvironment(Path.Combine(Directory.GetCurrentDirectory(), "Extensions", "TestNested"));

        private static IOptions<ManifestOptions> ManifestOptions =
            new StubManifestOptions(
                new ManifestOption { ManifestFileName = "Module.txt", Type = "module" },
                new ManifestOption { ManifestFileName = "Theme.txt", Type = "theme" }
                );

        private static IOptions<ExtensionExpanderOptions> ExtensionExpanderOptions =
            new StubExtensionExpanderOptions(
                new ExtensionExpanderOption { SearchPath = Path.Combine("Modules", "**") },
                new ExtensionExpanderOption { SearchPath = Path.Combine("Themes", "**") },
                new ExtensionExpanderOption { SearchPath = Path.Combine("AppData", "Sites", "**", "Modules", "**") },
                new ExtensionExpanderOption { SearchPath = Path.Combine("AppData", "Sites", "**", "Themes", "**") });

        private static IOptions<ShellOptions> ShellOptions =
            new StubShellOptions("AppData", "Sites");

        private static IEnumerable<IManifestProvider> ManifestProviders =
            new[] { new ManifestProvider(HostingEnvrionment) };

        private static IEnumerable<IExtensionProvider> ExtensionProviders =
            new[] {
                (IExtensionProvider)new ExtensionProvider(HostingEnvrionment, Enumerable.Empty<IFeaturesProvider>()),
                (IExtensionProvider)new ShellExtensionProvider(HostingEnvrionment, Enumerable.Empty<IFeaturesProvider>(), ShellOptions)
            };

        private IExtensionManager ExtensionManager;

        public ExtensionManagerNestingTests()
        {
            ExtensionManager = new ExtensionManager(
                ExtensionExpanderOptions,
                ManifestOptions,
                HostingEnvrionment,
                ManifestProviders,
                ExtensionProviders,
                Enumerable.Empty<IExtensionLoader>(),
                Enumerable.Empty<IExtensionOrderingStrategy>(),
                null,
                new NullLogger<ExtensionManager>(),
                null);
        }

        [Fact]
        public void ShouldReturnNestedExtensions()
        {
            var extensions = ExtensionManager.GetExtensions();
            var extensionIds = extensions.Select(o => o.Id).ToArray();

            Assert.Contains("Module1", extensionIds);
            Assert.Contains("Theme1", extensionIds);
            Assert.Contains("Site1_Module1", extensionIds);
            Assert.Contains("Site1_Theme1", extensionIds);
            Assert.Contains("Site2_Module1", extensionIds);
            Assert.Contains("Site2_Theme1", extensionIds);
        }

        [Fact]
        public void ShouldReturnNoExtensionsWhenNoOptionsSpecified()
        {
            var extensions = new ExtensionManager(
                    new StubExtensionExpanderOptions(),
                    ManifestOptions,
                    HostingEnvrionment,
                    Enumerable.Empty<IManifestProvider>(),
                    Enumerable.Empty<IExtensionProvider>(),
                    Enumerable.Empty<IExtensionLoader>(),
                    Enumerable.Empty<IExtensionOrderingStrategy>(),
                    null,
                    new NullLogger<ExtensionManager>(),
                    null)
                .GetExtensions();

            Assert.Empty(extensions);
        }

        [Fact]
        public void ShouldReturnNoExtensionsWhenNoMatches()
        {
            var extensions = new ExtensionManager(
                    new StubExtensionExpanderOptions(
                        new ExtensionExpanderOption { SearchPath = Path.Combine("NoFound", "**") }),
                    ManifestOptions,
                    HostingEnvrionment,
                    Enumerable.Empty<IManifestProvider>(),
                    Enumerable.Empty<IExtensionProvider>(),
                    Enumerable.Empty<IExtensionLoader>(),
                    Enumerable.Empty<IExtensionOrderingStrategy>(),
                    null,
                    new NullLogger<ExtensionManager>(),
                    null)
                .GetExtensions();

            Assert.Empty(extensions);
        }

        [Fact]
        public void ShouldReturnOnlyModule1()
        {
            var extensions = ExtensionManager.GetExtension("Module1");

            Assert.Equal("Module1", extensions.Id);
        }

        [Fact]
        public void ShouldReturnOnlyNestedModule1()
        {
            var extensions = ExtensionManager.GetExtension("Site1_Modules_Module1");

            Assert.Equal("Site1_Modules_Module1", extensions.Id);
        }
    }
}