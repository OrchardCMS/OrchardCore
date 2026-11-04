using System.Text.Json;
using Microsoft.Extensions.Hosting;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;

namespace OrchardCore.DataLocalization.Services;

/// <summary>
/// Lists the tab and card names of the <c>placement.json</c> rules of the enabled modules and themes, whatever their
/// filters.
/// </summary>
public sealed class PlacementFileTabAndCardNameContributor : ITabAndCardNameContributor
{
    private const string PlacementFileName = "placement.json";

    private readonly IHostEnvironment _hostEnvironment;
    private readonly IShellFeaturesManager _shellFeaturesManager;

    public PlacementFileTabAndCardNameContributor(
        IHostEnvironment hostEnvironment,
        IShellFeaturesManager shellFeaturesManager)
    {
        _hostEnvironment = hostEnvironment;
        _shellFeaturesManager = shellFeaturesManager;
    }

    public async Task<IEnumerable<string>> GetNamesAsync()
    {
        var names = new List<string>();
        var extensions = (await _shellFeaturesManager.GetEnabledFeaturesAsync())
            .Select(feature => feature.Extension)
            .DistinctBy(extension => extension.Id);

        foreach (var extension in extensions)
        {
            var fileInfo = _hostEnvironment.GetExtensionFileInfo(extension, PlacementFileName);
            if (!fileInfo.Exists)
            {
                continue;
            }

            await using var stream = fileInfo.CreateReadStream();
            var placementFile = await JsonSerializer.DeserializeAsync<PlacementFile>(stream, JOptions.Default);

            foreach (var node in placementFile?.Values.SelectMany(nodes => nodes ?? []) ?? [])
            {
                if (!string.IsNullOrEmpty(node.Location))
                {
                    names.AddRange(TabAndCardNames.FromLocation(node.Location));
                }
            }
        }

        return names;
    }
}
