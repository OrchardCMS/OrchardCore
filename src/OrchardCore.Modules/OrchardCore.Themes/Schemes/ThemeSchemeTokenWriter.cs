namespace OrchardCore.Themes.Schemes;

/// <summary>
/// Collects the CSS custom properties of one rule of a scheme (its light or its dark rule). Tokens are named under
/// the theme's prefix (<see cref="ThemeSchemeOptions.TokenPrefix"/>), following its renames
/// (<see cref="ThemeSchemeOptions.TokenNames"/>); <see cref="AddVariable"/> writes any other custom property, e.g. a
/// framework's. Writing a property again replaces its value in place.
/// </summary>
public sealed class ThemeSchemeTokenWriter
{
    private readonly IDictionary<string, string> _names;
    private readonly List<KeyValuePair<string, string>> _variables = [];
    private readonly Dictionary<string, int> _positions = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeSchemeTokenWriter"/> class.
    /// </summary>
    /// <param name="prefix">The prefix of the tokens, without dashes (e.g. <c>mytheme</c>).</param>
    /// <param name="names">The tokens the theme names otherwise, by generic name; optional.</param>
    public ThemeSchemeTokenWriter(string prefix, IDictionary<string, string> names = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(prefix);

        Prefix = prefix;
        _names = names ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the prefix of the tokens, without dashes.
    /// </summary>
    public string Prefix { get; }

    /// <summary>
    /// Gets the custom properties written so far, in order: full names (e.g. <c>--mytheme-primary</c>) and values.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> Variables
        => _variables;

    /// <summary>
    /// The custom property of a token: <c>--{prefix}-{name}</c>, with the name the theme gives it.
    /// </summary>
    public string NameOf(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);

        return $"--{Prefix}-{(_names.TryGetValue(token, out var renamed) && !string.IsNullOrEmpty(renamed) ? renamed : token)}";
    }

    /// <summary>
    /// A reference to a token, e.g. <c>var(--mytheme-primary)</c>.
    /// </summary>
    public string Var(string token)
        => $"var({NameOf(token)})";

    /// <summary>
    /// A reference to the <c>-rgb</c> triplet of a color token, e.g. <c>var(--mytheme-primary-rgb)</c>.
    /// </summary>
    public string VarRgb(string token)
        => $"var({NameOf(token)}-rgb)";

    /// <summary>
    /// Writes a token.
    /// </summary>
    public ThemeSchemeTokenWriter Add(string token, string value)
        => AddVariable(NameOf(token), value);

    /// <summary>
    /// Writes a color token, as '#rrggbb'.
    /// </summary>
    public ThemeSchemeTokenWriter Add(string token, ThemeColor color)
        => AddVariable(NameOf(token), color.ToHex());

    /// <summary>
    /// Writes the <c>-rgb</c> triplet of a color token ('r, g, b'), for <c>rgba(var(--{prefix}-{token}-rgb), 0.5)</c>.
    /// </summary>
    public ThemeSchemeTokenWriter AddRgb(string token, ThemeColor color)
        => AddVariable(NameOf(token) + "-rgb", color.ToTriplet());

    /// <summary>
    /// Writes a color token and its <c>-rgb</c> triplet.
    /// </summary>
    public ThemeSchemeTokenWriter AddWithRgb(string token, ThemeColor color)
        => Add(token, color).AddRgb(token, color);

    /// <summary>
    /// Writes any custom property, by its full name (e.g. <c>--bs-primary</c>).
    /// </summary>
    public ThemeSchemeTokenWriter AddVariable(string name, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (!name.StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"'{name}' is not the name of a custom property: it must start with '--'.", nameof(name));
        }

        if (_positions.TryGetValue(name, out var position))
        {
            _variables[position] = new(name, value);
        }
        else
        {
            _positions[name] = _variables.Count;
            _variables.Add(new(name, value));
        }

        return this;
    }
}
