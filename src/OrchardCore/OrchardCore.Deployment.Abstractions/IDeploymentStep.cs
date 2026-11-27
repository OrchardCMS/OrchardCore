using OrchardCore.Localization;

namespace OrchardCore.Deployment;

/// <summary>
/// Describes a <see cref="DeploymentStep"/> with the static members of its class, so the steps can be listed and grouped
/// without creating them. Every step implements it, and <c>services.AddDeployment&lt;TSource, TStep&gt;()</c> reads the members
/// of the step class when it registers the step.
/// </summary>
/// <remarks>
/// The members are never persisted: they are read from the type of the step, and translated at display time
/// with <c>IStringLocalizerFactory.Localize</c>, using the type of their source as the context.
/// </remarks>
public interface IDeploymentStep
{
    /// <summary>
    /// Gets the category of the step, used to group the steps of the picker. Return a source of
    /// <see cref="CommonLocalizationSources"/>, such as <see cref="CommonLocalizationSources.ContentManagement"/>.
    /// </summary>
    static abstract LocalizationSource Category { get; }

    /// <summary>
    /// Gets the title of the step, shown as the heading of its screens. Declare it, alongside <see cref="Category"/>,
    /// with a source of the module's class of localization sources. It is <see langword="null"/> by default, in which case
    /// the heading is derived from the name of the type of the step.
    /// </summary>
    static virtual LocalizationSource Title => null;
}
