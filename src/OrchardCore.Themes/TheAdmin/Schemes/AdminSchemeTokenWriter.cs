namespace OrchardCore.Themes.TheAdmin.Schemes;

/// <summary>
/// Collects the CSS custom properties of one rule of a scheme (its light or its dark rule). Tokens are named under
/// TheAdmin's prefix (<see cref="TheAdminSchemes.TokenPrefix"/>), following its renames
/// (<see cref="TheAdminSchemes.TokenNames"/>); <see cref="AddVariable"/> writes any other custom property. Writing a
/// property again replaces its value in place.
/// </summary>
public sealed class AdminSchemeTokenWriter
{
    private readonly IReadOnlyDictionary<string, string> _names = TheAdminSchemes.TokenNames;
    private readonly List<KeyValuePair<string, string>> _variables = [];
    private readonly Dictionary<string, int> _positions = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the custom properties written so far, in order: full names (e.g. <c>--ta-primary</c>) and values.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> Variables
        => _variables;

    /// <summary>
    /// The custom property of a token: <c>--{prefix}-{name}</c>, with the name the theme gives it.
    /// </summary>
    public string NameOf(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);

        return $"--{TheAdminSchemes.TokenPrefix}-{(_names.TryGetValue(token, out var renamed) && !string.IsNullOrEmpty(renamed) ? renamed : token)}";
    }

    /// <summary>
    /// Whether a token was written.
    /// </summary>
    public bool Contains(string token)
        => _positions.ContainsKey(NameOf(token));

    /// <summary>
    /// A reference to a token, e.g. <c>var(--ta-primary)</c>.
    /// </summary>
    public string Var(string token)
        => $"var({NameOf(token)})";

    /// <summary>
    /// A reference to the <c>-rgb</c> triplet of a color token, e.g. <c>var(--ta-primary-rgb)</c>.
    /// </summary>
    public string VarRgb(string token)
        => $"var({NameOf(token)}-rgb)";

    /// <summary>
    /// Writes a token.
    /// </summary>
    public AdminSchemeTokenWriter Add(string token, string value)
        => AddVariable(NameOf(token), value);

    /// <summary>
    /// Writes a color token, as '#rrggbb'.
    /// </summary>
    public AdminSchemeTokenWriter Add(string token, AdminColor color)
        => AddVariable(NameOf(token), color.ToHex());

    /// <summary>
    /// Writes the <c>-rgb</c> triplet of a color token ('r, g, b'), for <c>rgba(var(--{prefix}-{token}-rgb), 0.5)</c>.
    /// </summary>
    public AdminSchemeTokenWriter AddRgb(string token, AdminColor color)
        => AddVariable(NameOf(token) + "-rgb", color.ToTriplet());

    /// <summary>
    /// Writes a color token and its <c>-rgb</c> triplet.
    /// </summary>
    public AdminSchemeTokenWriter AddWithRgb(string token, AdminColor color)
        => Add(token, color).AddRgb(token, color);

    /// <summary>
    /// Writes any custom property, by its full name (e.g. <c>--bs-primary</c>).
    /// </summary>
    public AdminSchemeTokenWriter AddVariable(string name, string value)
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
