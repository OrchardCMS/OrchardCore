using Microsoft.Extensions.OptionsModel;
using Orchard.Environment.Extensions.Folders;
using Orchard.Environment.Extensions.Models;
using Orchard.Tests.Stubs;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Orchard.Tests.Hosting.Environment.Extensions
{
    public class ExtensionLocatorTests : IDisposable
    {
        private string _tempFolderName;

        public ExtensionLocatorTests()
        {
            _tempFolderName = Path.GetTempFileName();
            File.Delete(_tempFolderName);
            var assembly = GetType().GetTypeInfo().Assembly;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Hosting\\Environment\\Extensions\\FoldersData");
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (var file in di.GetFiles("*.txt", SearchOption.AllDirectories))
            {
                var targetPath = file.FullName.Replace(path, _tempFolderName);

                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                file.CopyTo(targetPath);
            }
        }

        public void Dispose()
        {
            Directory.Delete(_tempFolderName, true);
        }

        [Fact]
        [Trait("Category", "ExtensionLocator")]
        public void IdsFromFoldersWithModuleTxtShouldBeListed()
        {
            var harvester = new ExtensionHarvester(new StubWebSiteFolder(), new NullLogger<ExtensionHarvester>());
            var options = new ExtensionHarvestingOptions();
            options.ModuleLocationExpanders.Add(ModuleFolder(_tempFolderName));
            var folders = new ExtensionLocator(
                new FakeOptions(options),
                harvester);

            var ids = folders.AvailableExtensions().Select(d => d.Id);
            Assert.Equal(5, ids.Count());
            Assert.Contains("Sample1", ids); // Sample1 - obviously
            Assert.Contains("Sample3", ids); // Sample3
            Assert.Contains("Sample4", ids); // Sample4
            Assert.Contains("Sample6", ids); // Sample6
            Assert.Contains("Sample7", ids); // Sample7
        }

        [Fact]
        public void ModuleTxtShouldBeParsedAndReturnedAsYamlDocument()
        {
            var harvester = new ExtensionHarvester(new StubWebSiteFolder(), new NullLogger<ExtensionHarvester>());
            var options = new ExtensionHarvestingOptions();
            options.ModuleLocationExpanders.Add(ModuleFolder(_tempFolderName));
            var folders = new ExtensionLocator(
                new FakeOptions(options),
                harvester);

            var sample1 = folders.AvailableExtensions().Single(d => d.Id == "Sample1");
            Assert.NotEmpty(sample1.Id);
            Assert.Equal("Bertrand Le Roy", sample1.Author); // Sample1
        }

        [Fact]
        public void NamesFromFoldersWithModuleTxtShouldFallBackToIdIfNotGiven()
        {
            var harvester = new ExtensionHarvester(new StubWebSiteFolder(), new NullLogger<ExtensionHarvester>());
            var options = new ExtensionHarvestingOptions();
            options.ModuleLocationExpanders.Add(ModuleFolder(_tempFolderName));
            var folders = new ExtensionLocator(
                new FakeOptions(options),
                harvester);

            var names = folders.AvailableExtensions().Select(d => d.Name);
            Assert.Equal(5, names.Count());
            Assert.Contains("Le plug-in français", names); // Sample1
            Assert.Contains("This is another test.txt", names); // Sample3
            Assert.Contains("Sample4", names); // Sample4
            Assert.Contains("SampleSix", names); // Sample6
            Assert.Contains("Sample7", names); // Sample7
        }

        [Fact]
        public void PathsFromFoldersWithModuleTxtShouldFallBackAppropriatelyIfNotGiven()
        {
            var harvester = new ExtensionHarvester(new StubWebSiteFolder(), new NullLogger<ExtensionHarvester>());
            var options = new ExtensionHarvestingOptions();
            options.ModuleLocationExpanders.Add(ModuleFolder(_tempFolderName));
            var folders = new ExtensionLocator(
                new FakeOptions(options),
                harvester);

            var paths = folders.AvailableExtensions().Select(d => d.Path);
            Assert.Equal(5, paths.Count());
            Assert.Contains("Sample1", paths); // Sample1 - Id, Name invalid URL segment
            Assert.Contains("Sample3", paths); // Sample3 - Id, Name invalid URL segment
            Assert.Contains("ThisIs.Sample4", paths); // Sample4 - Path
            Assert.Contains("SampleSix", paths); // Sample6 - Name, no Path
            Assert.Contains("Sample7", paths); // Sample7 - Id, no Name or Path
        }
        public ModuleLocationExpander ModuleFolder(string path)
        {
            return new ModuleLocationExpander(
                DefaultExtensionTypes.Module,
                new[] { path },
                "Module.txt"
                );
        }

        public class FakeOptions : IOptions<ExtensionHarvestingOptions>
        {
            public FakeOptions(ExtensionHarvestingOptions options)
            {
                Value = options;
            }
            public ExtensionHarvestingOptions Value
            {
                get;
                private set;
            }
        }
    }
}