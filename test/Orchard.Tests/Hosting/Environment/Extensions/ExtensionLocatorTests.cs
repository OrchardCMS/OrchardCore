using Microsoft.Framework.OptionsModel;
using Orchard.Hosting.Extensions.Folders;
using Orchard.Hosting.Extensions.Models;
using Orchard.Tests.Stubs;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Orchard.Tests.Hosting.Environment.Extensions {
    public class ExtensionLocatorTests : IDisposable
    {
        private string _tempFolderName;

        public ExtensionLocatorTests() {
            _tempFolderName = Path.GetTempFileName();
            File.Delete(_tempFolderName);
            var assembly = GetType().GetTypeInfo().Assembly;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Hosting\\Environment\\Extensions\\FoldersData");
            DirectoryInfo di = new DirectoryInfo(path);
            foreach (var file in di.GetFiles("*.txt",SearchOption.AllDirectories)) {
                var targetPath = file.FullName.Replace(path, _tempFolderName);
                
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                file.CopyTo(targetPath);
            }
        }
        
        public void Dispose() {
            Directory.Delete(_tempFolderName, true);
        }

        [Fact]
        public void IdsFromFoldersWithModuleTxtShouldBeListed() {
            var harvester = new ExtensionHarvester(new StubWebSiteFolder(), new StubLoggerFactory());
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
        
        public ModuleLocationExpander ModuleFolder(string path) {
                return  new ModuleLocationExpander(
                    DefaultExtensionTypes.Module,
                    new[] { path },
                    "Module.txt"
                    );
        }

        public class FakeOptions : IOptions<ExtensionHarvestingOptions> {
            public FakeOptions(ExtensionHarvestingOptions options) {
                Value = options;
            }
            public ExtensionHarvestingOptions Value {
                get;
                private set;
            }
        }
    }
}
