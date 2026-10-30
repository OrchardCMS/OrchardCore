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
    /// Gets the human-readable description of the counts selecting this form.
    /// </summary>
    public string Description => Condition.Description;

    /// <summary>
    /// Gets example counts selecting this form, computed by evaluating the runtime <see cref="PluralizationRuleDelegate"/>.
    /// </summary>
    public IReadOnlyList<int> Examples { get; }
}
