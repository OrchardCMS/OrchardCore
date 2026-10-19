using System.Text.Json.Serialization;
using OrchardCore.Localization;

namespace OrchardCore.Deployment;

public abstract class DeploymentStep
{
    private static readonly LocalizationSource s_emptyCategory = LocalizationSource.Create(string.Empty);

    public string Id { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// The category of the step, used to group the steps of the picker. Override it with a source of
    /// <see cref="CommonLocalizationSources"/>, such as <see cref="CommonLocalizationSources.ContentManagement"/>.
    /// </summary>
    /// <remarks>
    /// The category is translated at display time using its source type as the context. It is never persisted:
    /// the value is always read from the step's type.
    /// </remarks>
    [JsonIgnore]
    public virtual LocalizationSource Category => s_emptyCategory;

    /// <summary>
    /// The title of the step, shown as the heading of its screens. Override it, alongside <see cref="Category"/>,
    /// with a source declared in the module's class of localization sources.
    /// </summary>
    /// <remarks>
    /// The title is translated at display time using its source type as the context. It is never persisted,
    /// for the reasons given on <see cref="Category"/>.
    /// </remarks>
    [JsonIgnore]
    public virtual LocalizationSource Title => null;
}
