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
        private const string DataPrefix = "Orchard.Tests.Environment.Extensions.FoldersData.";
        private string _tempFolderName;

        public ExtensionLocatorTests() {
            _tempFolderName = Path.GetTempFileName();
            File.Delete(_tempFolderName);
            var assembly = GetType().GetTypeInfo().Assembly;
            foreach (var name in assembly.GetManifestResourceNames()) {
                if (name.StartsWith(DataPrefix)) {
                    var text = "";
                    using (var stream = assembly.GetManifestResourceStream(name)) {
                        using (var reader = new StreamReader(stream))
                            text = reader.ReadToEnd();

                    }

                    var relativePath = name
                        .Substring(DataPrefix.Length)
                        .Replace(".txt", ":txt")
                        .Replace('.', Path.DirectorySeparatorChar)
                        .Replace(":txt", ".txt");

                    var targetPath = Path.Combine(_tempFolderName, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    using (var stream = new FileStream(targetPath, FileMode.Create)) {
                        using (var writer = new StreamWriter(stream)) {
                            writer.Write(text);
                        }
                    }
                }
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
