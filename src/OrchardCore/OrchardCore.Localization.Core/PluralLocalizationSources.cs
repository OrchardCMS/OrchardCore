namespace OrchardCore.Localization;

/// <summary>
/// Untranslated strings describing the plural rules. Their PO context is <c>OrchardCore.Localization.PluralLocalizationSources</c>.
/// </summary>
/// <remarks>
/// The descriptions of a <see cref="PluralCondition"/> are composed from these sources. The sentence forms start a
/// description, and the inline forms are used within another description, e.g. after "except".
/// </remarks>
internal static class PluralLocalizationSources
{
    /// <summary>Gets the "All counts" text.</summary>
    public static readonly LocalizationSource AllCounts = LocalizationSource.Create(
        "All counts",
        typeof(PluralLocalizationSources),
        "Plural rule condition of a language having a single plural form.");

    /// <summary>Gets the "All remaining counts" text.</summary>
    public static readonly LocalizationSource AllRemainingCounts = LocalizationSource.Create(
        "All remaining counts",
        typeof(PluralLocalizationSources),
        "Plural rule condition of the form used for the counts not matched by the other forms.");

    /// <summary>Gets the "Exactly {0}" text.</summary>
    public static readonly LocalizationSource Exactly = LocalizationSource.Create(
        "Exactly {0}",
        typeof(PluralLocalizationSources),
        "Plural rule condition starting a sentence. {0} is a number or a list of numbers, e.g. \"0 or 1\".");

    /// <summary>Gets the "exactly {0}" text.</summary>
    public static readonly LocalizationSource ExactlyInline = LocalizationSource.Create(
        "exactly {0}",
        typeof(PluralLocalizationSources),
        "Plural rule condition within a sentence, e.g. \"Ends in 1, except exactly 1\". {0} is a number or a list of numbers.");

    /// <summary>Gets the "From {0} to {1}" text.</summary>
    public static readonly LocalizationSource Between = LocalizationSource.Create(
        "From {0} to {1}",
        typeof(PluralLocalizationSources),
        "Plural rule condition starting a sentence, matching the counts from {0} to {1} inclusive.");

    /// <summary>Gets the "from {0} to {1}" text.</summary>
    public static readonly LocalizationSource BetweenInline = LocalizationSource.Create(
        "from {0} to {1}",
        typeof(PluralLocalizationSources),
        "Plural rule condition within a sentence, e.g. \"Exactly 0 or 1, or from 11 to 99\", matching the counts from {0} to {1} inclusive.");

    /// <summary>Gets the "Ends in {0}" text.</summary>
    public static readonly LocalizationSource EndsIn = LocalizationSource.Create(
        "Ends in {0}",
        typeof(PluralLocalizationSources),
        "Plural rule condition starting a sentence, matching the counts whose last digits are {0}, e.g. \"2-4\" or \"01\".");

    /// <summary>Gets the "ends in {0}" text.</summary>
    public static readonly LocalizationSource EndsInInline = LocalizationSource.Create(
        "ends in {0}",
        typeof(PluralLocalizationSources),
        "Plural rule condition within a sentence, matching the counts whose last digits are {0}, e.g. \"2-4\" or \"01\".");

    /// <summary>Gets the "{0}-{1}" text.</summary>
    public static readonly LocalizationSource DigitsRange = LocalizationSource.Create(
        "{0}-{1}",
        typeof(PluralLocalizationSources),
        "A range of trailing digits in a plural rule condition, e.g. \"12-14\" in \"Ends in 12-14\".");

    /// <summary>Gets the "Multiple of {0}" text.</summary>
    public static readonly LocalizationSource MultipleOf = LocalizationSource.Create(
        "Multiple of {0}",
        typeof(PluralLocalizationSources),
        "Plural rule condition starting a sentence, matching the multiples of the number {0}.");

    /// <summary>Gets the "multiple of {0}" text.</summary>
    public static readonly LocalizationSource MultipleOfInline = LocalizationSource.Create(
        "multiple of {0}",
        typeof(PluralLocalizationSources),
        "Plural rule condition within a sentence, e.g. \"Ends in 01000-20000 and multiple of 1,000\", matching the multiples of the number {0}.");

    /// <summary>Gets the "{0}, or {1}" text.</summary>
    public static readonly LocalizationSource Or = LocalizationSource.Create(
        "{0}, or {1}",
        typeof(PluralLocalizationSources),
        "Combines two plural rule conditions, matching the counts matched by either of them.");

    /// <summary>Gets the "{0} and {1}" text.</summary>
    public static readonly LocalizationSource And = LocalizationSource.Create(
        "{0} and {1}",
        typeof(PluralLocalizationSources),
        "Combines two plural rule conditions, matching the counts matched by both of them.");

    /// <summary>Gets the "{0}, except {1}" text.</summary>
    public static readonly LocalizationSource Except = LocalizationSource.Create(
        "{0}, except {1}",
        typeof(PluralLocalizationSources),
        "Plural rule condition {0} without the counts of {1}, e.g. \"Ends in 1, except 11\".");

    /// <summary>Gets the "{0}, and except {1}" text.</summary>
    public static readonly LocalizationSource AndExcept = LocalizationSource.Create(
        "{0}, and except {1}",
        typeof(PluralLocalizationSources),
        "Plural rule condition {0}, already having an exception, without the counts of {1}.");

    /// <summary>Gets the "{0}, {1}" text.</summary>
    public static readonly LocalizationSource ListSeparator = LocalizationSource.Create(
        "{0}, {1}",
        typeof(PluralLocalizationSources),
        "Separates the values of a list in a plural rule condition, except the last two, e.g. \"1, 21\" in \"1, 21 or 41\".");

    /// <summary>Gets the "{0} or {1}" text.</summary>
    public static readonly LocalizationSource ListLast = LocalizationSource.Create(
        "{0} or {1}",
        typeof(PluralLocalizationSources),
        "Joins the last value {1} of a list in a plural rule condition, e.g. \"0 or 1\", or \"1, 21 or 41\".");
}
