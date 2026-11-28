using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization;

/// <summary>
/// Describes one plural form of a <see cref="PluralRuleDescriptor"/>, i.e. one <c>msgstr[Index]</c> translation of a PO entry.
/// </summary>
public sealed class PluralFormDescriptor
{
    internal PluralFormDescriptor(int index, PluralCondition condition, IReadOnlyList<int> examples)
    {
        Index = index;
        Condition = condition;
        Examples = examples;
    }

    /// <summary>
    /// Gets the index returned by the <see cref="PluralizationRuleDelegate"/> for this form, which is also the index of
    /// the <c>msgstr[Index]</c> translation used in PO files.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets the condition selecting this form.
    /// </summary>
    public PluralCondition Condition { get; }

    /// <summary>
    /// Gets the untranslated human-readable description of the counts selecting this form. Use
    /// <see cref="GetDescription(IStringLocalizerFactory)"/> to get a translated description.
    /// </summary>
    public string Description => Condition.Description;

    /// <summary>
    /// Gets example counts selecting this form, computed by evaluating the runtime <see cref="PluralizationRuleDelegate"/>.
    /// </summary>
    public IReadOnlyList<int> Examples { get; }

    /// <summary>
    /// Returns the description of the counts selecting this form, translated with the current UI culture.
    /// </summary>
    /// <param name="stringLocalizerFactory">The factory creating the localizer of the description.</param>
    public string GetDescription(IStringLocalizerFactory stringLocalizerFactory)
        => Condition.GetDescription(stringLocalizerFactory);
}
