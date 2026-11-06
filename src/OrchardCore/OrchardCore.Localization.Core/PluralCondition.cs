using System.Globalization;
using Microsoft.Extensions.Localization;
using Sources = OrchardCore.Localization.PluralLocalizationSources;

namespace OrchardCore.Localization;

/// <summary>
/// Describes, in a human-readable and evaluable way, which counts select a plural form of a <see cref="PluralRuleDescriptor"/>.
/// </summary>
/// <remarks>
/// The description is built from the same building blocks as <see cref="IsMatch(int)"/>, so that the text can be
/// validated against the runtime <see cref="PluralizationRuleDelegate"/>. Each building block is a localizable
/// <see cref="LocalizationSource"/>, so the description can be translated with <see cref="GetDescription(IStringLocalizerFactory)"/>.
/// Like the CLDR operands, conditions apply to the absolute value of the count.
/// </remarks>
public sealed class PluralCondition
{
    private enum Kind
    {
        Remaining,
        Exactly,
        Range,
        Ending,
        Multiple,
        Composite,
        Exception,
    }

    private readonly Func<long, bool> _predicate;
    private readonly Kind _kind;

    // Renders the description, as a sentence or inline within another description.
    private readonly Func<DescriptionRenderer, bool, string> _render;

    // Renders the values without their prefix, e.g. "12-14" for "Ends in 12-14", used to shorten combined descriptions.
    private readonly Func<DescriptionRenderer, IReadOnlyList<string>> _values;

    private readonly Lazy<string> _description;

    private PluralCondition(
        Func<DescriptionRenderer, bool, string> render,
        Func<long, bool> predicate,
        Kind kind,
        Func<DescriptionRenderer, IReadOnlyList<string>> values = null)
    {
        _render = render;
        _predicate = predicate;
        _kind = kind;
        _values = values;
        _description = new(() => _render(DescriptionRenderer.Invariant, false));
    }

    /// <summary>
    /// Gets a condition matching every count, for a rule with a single plural form.
    /// </summary>
    public static PluralCondition All { get; } = new((r, _) => r.Format(Sources.AllCounts), static _ => false, Kind.Remaining);

    /// <summary>
    /// Gets a condition matching the counts that are not matched by any other form of the rule.
    /// </summary>
    public static PluralCondition Remaining { get; } = new((r, _) => r.Format(Sources.AllRemainingCounts), static _ => false, Kind.Remaining);

    /// <summary>
    /// Gets the untranslated human-readable description of the condition, e.g. "Ends in 2-4, except 12-14", with the
    /// numbers formatted with the invariant culture. Use <see cref="GetDescription(IStringLocalizerFactory)"/> to
    /// get a translated description.
    /// </summary>
    public string Description => _description.Value;

    /// <summary>
    /// Gets whether the condition matches the counts that are not matched by any other form of the rule.
    /// </summary>
    public bool IsRemaining => _kind == Kind.Remaining;

    /// <summary>
    /// Creates a condition matching the specified counts exactly.
    /// </summary>
    /// <param name="values">The matching counts.</param>
    public static PluralCondition Exactly(params long[] values)
    {
        EnsureValues(values);

        var copy = values.ToArray();

        IReadOnlyList<string> Texts(DescriptionRenderer r) => copy.Select(r.Number).ToArray();

        return new(
            (r, inline) => r.Format(inline ? Sources.ExactlyInline : Sources.Exactly, r.List(Texts(r))),
            n => copy.Contains(n),
            Kind.Exactly,
            Texts);
    }

    /// <summary>
    /// Creates a condition matching the counts from <paramref name="min"/> to <paramref name="max"/>, inclusive.
    /// </summary>
    /// <param name="min">The lowest matching count.</param>
    /// <param name="max">The highest matching count.</param>
    public static PluralCondition Between(long min, long max)
    {
        EnsureRange(min, max, long.MaxValue);

        return new(
            (r, inline) => r.Format(inline ? Sources.BetweenInline : Sources.Between, r.Number(min), r.Number(max)),
            n => n >= min && n <= max,
            Kind.Range);
    }

    /// <summary>
    /// Creates a condition matching the counts whose last <paramref name="digits"/> digits, including leading zeros,
    /// are one of <paramref name="values"/>. For instance <c>EndsIn(2, 1)</c> matches 1, 101 and 201, but not 11.
    /// </summary>
    /// <param name="digits">The number of trailing digits, from 1 to 9.</param>
    /// <param name="values">The trailing digits values.</param>
    public static PluralCondition EndsIn(int digits, params long[] values)
    {
        EnsureValues(values);

        var modulo = GetModulo(digits);

        foreach (var value in values)
        {
            EnsureRange(value, value, modulo - 1);
        }

        var copy = values.ToArray();

        return CreateEnding(r => copy.Select(v => r.Digits(v, digits)).ToArray(), n => copy.Contains(n % modulo));
    }

    /// <summary>
    /// Creates a condition matching the counts whose last <paramref name="digits"/> digits, including leading zeros,
    /// are from <paramref name="min"/> to <paramref name="max"/>.
    /// </summary>
    /// <param name="digits">The number of trailing digits, from 1 to 9.</param>
    /// <param name="min">The lowest trailing digits value.</param>
    /// <param name="max">The highest trailing digits value.</param>
    public static PluralCondition EndsInRange(int digits, long min, long max)
    {
        var modulo = GetModulo(digits);

        EnsureRange(min, max, modulo - 1);

        return CreateEnding(
            r => [r.Format(Sources.DigitsRange, r.Digits(min, digits), r.Digits(max, digits))],
            n => n % modulo >= min && n % modulo <= max);
    }

    /// <summary>
    /// Creates a condition matching the multiples of <paramref name="divisor"/>, including 0.
    /// </summary>
    /// <param name="divisor">The positive divisor.</param>
    public static PluralCondition MultipleOf(long divisor)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(divisor);

        return new(
            (r, inline) => r.Format(inline ? Sources.MultipleOfInline : Sources.MultipleOf, r.Number(divisor)),
            n => n % divisor == 0,
            Kind.Multiple);
    }

    /// <summary>
    /// Creates a condition matching the counts matched by this condition or by <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The alternative condition.</param>
    public PluralCondition Or(PluralCondition other)
    {
        EnsureCombinable(this, other);

        // "Ends in 0 or 11-19" reads better than "Ends in 0, or ends in 11-19".
        if (_kind == Kind.Ending && other._kind == Kind.Ending)
        {
            return CreateEnding(r => [.. _values(r), .. other._values(r)], n => _predicate(n) || other._predicate(n));
        }

        return new(
            (r, inline) => r.Format(Sources.Or, _render(r, inline), other._render(r, true)),
            n => _predicate(n) || other._predicate(n),
            Kind.Composite);
    }

    /// <summary>
    /// Creates a condition matching the counts matched by both this condition and <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other condition.</param>
    public PluralCondition And(PluralCondition other)
    {
        EnsureCombinable(this, other);

        return new(
            (r, inline) => r.Format(Sources.And, _render(r, inline), other._render(r, true)),
            n => _predicate(n) && other._predicate(n),
            Kind.Composite);
    }

    /// <summary>
    /// Creates a condition matching the counts matched by this condition but not by <paramref name="exclusion"/>.
    /// </summary>
    /// <param name="exclusion">The condition of the excluded counts.</param>
    public PluralCondition Except(PluralCondition exclusion)
    {
        EnsureCombinable(this, exclusion);

        // "Ends in 1, except 11" reads better than "Ends in 1, except ends in 11", and "From 3 to 19, except 11"
        // better than "From 3 to 19, except exactly 11". The prefix is kept when the values could be misread,
        // e.g. "Ends in 1, except exactly 1".
        var isShort =
            (_kind == Kind.Ending && exclusion._kind == Kind.Ending) ||
            (_kind is Kind.Range or Kind.Multiple && exclusion._kind == Kind.Exactly);

        var source = _kind == Kind.Exception ? Sources.AndExcept : Sources.Except;

        return new(
            (r, inline) => r.Format(
                source,
                _render(r, inline),
                isShort ? r.List(exclusion._values(r)) : exclusion._render(r, true)),
            n => _predicate(n) && !exclusion._predicate(n),
            Kind.Exception);
    }

    /// <summary>
    /// Returns the description of the condition translated with the current UI culture, with the numbers formatted
    /// with the current culture.
    /// </summary>
    /// <param name="stringLocalizerFactory">The factory creating the localizer of the description.</param>
    public string GetDescription(IStringLocalizerFactory stringLocalizerFactory)
    {
        ArgumentNullException.ThrowIfNull(stringLocalizerFactory);

        return _render(new DescriptionRenderer(stringLocalizerFactory, CultureInfo.CurrentCulture), false);
    }

    /// <summary>
    /// Returns whether the count is matched by the condition. A <see cref="IsRemaining"/> condition never matches,
    /// as it depends on the other forms of the rule.
    /// </summary>
    /// <param name="count">The count, of which the absolute value is used.</param>
    public bool IsMatch(int count) => !IsRemaining && _predicate(Math.Abs((long)count));

    /// <inheritdoc />
    public override string ToString() => Description;

    private static PluralCondition CreateEnding(Func<DescriptionRenderer, IReadOnlyList<string>> values, Func<long, bool> predicate)
        => new(
            (r, inline) => r.Format(inline ? Sources.EndsInInline : Sources.EndsIn, r.List(values(r))),
            predicate,
            Kind.Ending,
            values);

    private static long GetModulo(int digits)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(digits, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(digits, 9);

        long modulo = 1;

        for (var i = 0; i < digits; i++)
        {
            modulo *= 10;
        }

        return modulo;
    }

    private static void EnsureValues(long[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Length == 0)
        {
            throw new ArgumentException("At least one value is required.", nameof(values));
        }
    }

    private static void EnsureRange(long min, long max, long limit)
    {
        if (min < 0 || max > limit || min > max)
        {
            throw new ArgumentOutOfRangeException(nameof(min), $"The range {min}-{max} is not valid.");
        }
    }

    private static void EnsureCombinable(PluralCondition left, PluralCondition right)
    {
        ArgumentNullException.ThrowIfNull(right);

        if (left.IsRemaining || right.IsRemaining)
        {
            throw new InvalidOperationException("A condition matching the remaining counts can't be combined.");
        }
    }

    // Formats the localization sources and numbers of a description, translated when a factory is provided.
    private sealed class DescriptionRenderer
    {
        public static readonly DescriptionRenderer Invariant = new(null, CultureInfo.InvariantCulture);

        private readonly IStringLocalizerFactory _factory;
        private readonly CultureInfo _culture;

        public DescriptionRenderer(IStringLocalizerFactory factory, CultureInfo culture)
        {
            _factory = factory;
            _culture = culture;
        }

        public string Format(LocalizationSource source, params string[] arguments)
            => _factory is null
                ? string.Format(CultureInfo.InvariantCulture, source.Value, arguments)
                : _factory.Localize(source, arguments).Value;

        public string Number(long value) => value.ToString("N0", _culture);

        public string Digits(long value, int digits) => value.ToString(new string('0', digits), _culture);

        public string List(IReadOnlyList<string> texts)
        {
            if (texts.Count == 1)
            {
                return texts[0];
            }

            var head = texts[0];

            for (var i = 1; i < texts.Count - 1; i++)
            {
                head = Format(Sources.ListSeparator, head, texts[i]);
            }

            return Format(Sources.ListLast, head, texts[^1]);
        }
    }
}
