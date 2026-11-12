using System.Text.Json;
using Microsoft.Extensions.Hosting;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Services;

/// <summary>
/// Provides the tab and card names of the <c>placement.json</c> rules of the enabled modules and themes, whatever
/// their filters.
/// </summary>
internal sealed class PlacementFileLocalizationDataProvider : ILocalizationDataProvider
{
    private const string PlacementFileName = "placement.json";

    private readonly IHostEnvironment _hostEnvironment;
    private readonly IShellFeaturesManager _shellFeaturesManager;

    public PlacementFileLocalizationDataProvider(
        IHostEnvironment hostEnvironment,
        IShellFeaturesManager shellFeaturesManager)
    {
        _hostEnvironment = hostEnvironment;
        _shellFeaturesManager = shellFeaturesManager;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
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

        return names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name => new DataLocalizedString(TabAndCardNames.DataLocalizationContext, name, string.Empty));
    }
}
