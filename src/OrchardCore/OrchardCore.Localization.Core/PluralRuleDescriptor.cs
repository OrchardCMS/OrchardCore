namespace OrchardCore.Localization;

/// <summary>
/// Describes a plural rule of the <see cref="DefaultPluralRuleProvider"/>: the cultures using it, its runtime
/// <see cref="PluralizationRuleDelegate"/>, and the condition and example counts of each of its plural forms.
/// </summary>
/// <remarks>
/// The forms are ordered by the index returned by the runtime rule, which is the <c>msgstr[n]</c> index of the PO files.
/// This ordering doesn't follow the order of the CLDR plural categories.
/// </remarks>
public sealed class PluralRuleDescriptor
{
    /// <summary>
    /// The maximum number of example counts computed for each form.
    /// </summary>
    public const int ExampleCount = 4;

    // The examples start with the first and last counts of each run of consecutive counts selecting a form, below this limit.
    private const int RunSearchLimit = 1000;

    private readonly IReadOnlyList<PluralCondition> _conditions;
    private readonly Lazy<IReadOnlyList<PluralFormDescriptor>> _forms;

    internal PluralRuleDescriptor(IReadOnlyList<string> cultures, PluralizationRuleDelegate rule, IReadOnlyList<PluralCondition> conditions)
    {
        Cultures = cultures;
        Rule = rule;
        _conditions = conditions;
        _forms = new(CreateForms);
    }

    /// <summary>
    /// Gets the names of the cultures registered with this rule. Other cultures use the rule of their closest
    /// registered parent culture.
    /// </summary>
    public IReadOnlyList<string> Cultures { get; }

    /// <summary>
    /// Gets the runtime rule returning the index of the plural form to use for a count.
    /// </summary>
    public PluralizationRuleDelegate Rule { get; }

    /// <summary>
    /// Gets the number of plural forms, i.e. the number of <c>msgstr[n]</c> translations of a PO entry.
    /// </summary>
    public int FormCount => _conditions.Count;

    /// <summary>
    /// Gets the plural forms, ordered by their <see cref="PluralFormDescriptor.Index"/>.
    /// </summary>
    public IReadOnlyList<PluralFormDescriptor> Forms => _forms.Value;

    // The non-negative counts from which the examples are taken, in ascending order.
    private static IEnumerable<int> GetExampleCandidates()
    {
        for (var count = 0; count < 10_000; count++)
        {
            yield return count;
        }

        for (var count = 10_000; count < 10_000_000; count += 1_000)
        {
            yield return count;
        }

        for (var count = 10_000_000; count <= 2_000_000_000; count += 1_000_000)
        {
            yield return count;
        }
    }

    private PluralFormDescriptor[] CreateForms()
    {
        var examples = ComputeExamples(Rule, FormCount);

        return _conditions
            .Select((condition, index) => new PluralFormDescriptor(index, condition, examples[index]))
            .ToArray();
    }

    private static List<int>[] ComputeExamples(PluralizationRuleDelegate rule, int formCount)
    {
        var examples = new List<int>[formCount];

        for (var i = 0; i < formCount; i++)
        {
            examples[i] = [];
        }

        var start = 0;

        while (start < RunSearchLimit)
        {
            var index = rule(start);
            var end = start;

            while (end + 1 < RunSearchLimit && rule(end + 1) == index)
            {
                end++;
            }

            if (index >= 0 && index < formCount)
            {
                Add(examples[index], start);

                // The end of a run reaching the limit isn't meaningful.
                if (end + 1 < RunSearchLimit)
                {
                    Add(examples[index], end);
                }
            }

            start = end + 1;
        }

        // Fill the forms selected by few runs with the next counts, e.g. "2, 3, 4, 5" rather than "2", and find the
        // forms only selected by large counts, e.g. the multiples of 1,000,000.
        foreach (var count in GetExampleCandidates())
        {
            if (examples.All(e => e.Count >= ExampleCount))
            {
                break;
            }

            var index = rule(count);

            if (index >= 0 && index < formCount)
            {
                Add(examples[index], count);
            }
        }

        foreach (var list in examples)
        {
            list.Sort();
        }

        return examples;

        static void Add(List<int> list, int count)
        {
            if (list.Count < ExampleCount && !list.Contains(count))
            {
                list.Add(count);
            }
        }
    }
}
