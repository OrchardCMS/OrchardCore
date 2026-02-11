using System.Collections.Generic;

namespace OrchardCore.Localization.Data;

/// <summary>
/// A no-op implementation of <see cref="IDataLocalizer"/> that returns the original values without translation.
/// This is used when the OrchardCore.DataLocalization feature is not enabled.
/// </summary>
public sealed class NullDataLocalizer : IDataLocalizer
{
    /// <summary>
    /// Returns the shared instance of <see cref="NullDataLocalizer"/>.
    /// </summary>
    public static NullDataLocalizer Instance { get; } = new NullDataLocalizer();

    /// <inheritdoc/>
    public DataLocalizedString this[string name, string context]
    {
        get
        {
            ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));
            ArgumentNullException.ThrowIfNullOrEmpty(context, nameof(context));

            return new DataLocalizedString(context, name, name, resourceNotFound: true);
        }
    }

    /// <inheritdoc/>
    public DataLocalizedString this[string name, string context, params object[] arguments]
    {
        get
        {
            ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));
            ArgumentNullException.ThrowIfNullOrEmpty(context, nameof(context));

            var formatted = arguments?.Length > 0 ? string.Format(name, arguments) : name;

            return new DataLocalizedString(context, name, formatted, resourceNotFound: true);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<DataLocalizedString> GetAllStrings(string context, bool includeParentCultures)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(context, nameof(context));

        return [];
    }
}
