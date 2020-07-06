using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Placements.Services
{
    public class PlacementFileStore : IPlacementFileStore
    {
        private readonly IOptions<ShellOptions> _shellOptions;
        private readonly ShellSettings _shellSettings;

        public PlacementFileStore(
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings)
        {
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
        }

        /// <summary>
        /// Loads site placement file
        /// </summary>
        public Task<PlacementFile> LoadPlacementFileAsync()
        {
            PlacementFile result;

            if (!File.Exists(Filename))
            {
                result = new PlacementFile();
            }
            else
            {
                lock (this)
                {
                    using (var file = File.OpenText(Filename))
                    {
                        using (var jtr = new JsonTextReader(file))
                        {
                            var serializer = new JsonSerializer();
                            result = serializer.Deserialize<PlacementFile>(jtr);
                        }
                    }
                }
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// Stores site placement file
        /// </summary>
        public Task SavePlacementFileAsync(PlacementFile placementFile)
        {
            lock (this)
            {
                using (var file = File.CreateText(Filename))
                {
                    var serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(file, placementFile);
                }
            }

            return Task.CompletedTask;
        }

        private string Filename => PathExtensions.Combine(
            _shellOptions.Value.ShellsApplicationDataPath,
            _shellOptions.Value.ShellsContainerName,
            _shellSettings.Name, "placement.json");
    }
}
