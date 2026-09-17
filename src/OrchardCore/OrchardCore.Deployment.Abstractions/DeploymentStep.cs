using System.Text.Json.Serialization;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment;

public abstract class DeploymentStep
{
    public string Id { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// The localized category of the step, used to group the steps of the picker. Set it in the step's constructor.
    /// </summary>
    /// <remarks>
    /// It is never persisted. A step is rehydrated through its parameterless constructor, which doesn't resolve a
    /// localizer, so the value is always read from a fresh instance built by the step's factory instead. Persisting it
    /// would also break the plan, because <see cref="LocalizedString"/> has no constructor
    /// <see cref="System.Text.Json.JsonSerializer"/> can call.
    /// </remarks>
    [JsonIgnore]
    public LocalizedString Category { get; set; } = new(string.Empty, string.Empty);

    /// <summary>
    /// The localized title of the step, shown as the heading of its screens. Set it in the step's constructor,
    /// alongside <see cref="Category"/>.
    /// </summary>
    /// <remarks>
    /// It is never persisted, for the reasons given on <see cref="Category"/>.
    /// </remarks>
    [JsonIgnore]
    public LocalizedString Title { get; set; }
}
