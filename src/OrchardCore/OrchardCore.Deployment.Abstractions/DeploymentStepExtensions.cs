using Microsoft.Extensions.Localization;

namespace OrchardCore.Deployment;

/// <summary>
/// Provides extension methods for <see cref="DeploymentStep"/>.
/// </summary>
public static class DeploymentStepExtensions
{
    /// <summary>
    /// Translates the <see cref="DeploymentStep.Category"/> of a step.
    /// </summary>
    /// <param name="step">The <see cref="DeploymentStep"/>.</param>
    /// <param name="stringLocalizerFactory">The <see cref="IStringLocalizerFactory"/>.</param>
    /// <returns>The translated category, or the category when it has no name.</returns>
    /// <remarks>
    /// The category is translated by its name, with the step type as the context. This is the same context that
    /// <see cref="IStringLocalizer{T}"/> of the step type uses.
    /// </remarks>
    public static LocalizedString GetLocalizedCategory(this DeploymentStep step, IStringLocalizerFactory stringLocalizerFactory)
        => Localize(step, step?.Category, stringLocalizerFactory);

    /// <summary>
    /// Translates the <see cref="DeploymentStep.Title"/> of a step.
    /// </summary>
    /// <param name="step">The <see cref="DeploymentStep"/>.</param>
    /// <param name="stringLocalizerFactory">The <see cref="IStringLocalizerFactory"/>.</param>
    /// <returns>The translated title, or the title when it has no name.</returns>
    /// <remarks>
    /// The title is translated by its name, with the step type as the context. This is the same context that
    /// <see cref="IStringLocalizer{T}"/> of the step type uses.
    /// </remarks>
    public static LocalizedString GetLocalizedTitle(this DeploymentStep step, IStringLocalizerFactory stringLocalizerFactory)
        => Localize(step, step?.Title, stringLocalizerFactory);

    private static LocalizedString Localize(DeploymentStep step, LocalizedString value, IStringLocalizerFactory stringLocalizerFactory)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(stringLocalizerFactory);

        if (string.IsNullOrEmpty(value?.Name))
        {
            return value;
        }

        return stringLocalizerFactory.Create(step.GetType())[value.Name];
    }
}
