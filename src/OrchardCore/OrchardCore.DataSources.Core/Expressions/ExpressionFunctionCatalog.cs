namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// The functions of the registered <see cref="IExpressionFunctionProvider"/> implementations, gathered and checked
/// the first time they are used. Registered as a singleton.
/// </summary>
public sealed class ExpressionFunctionCatalog : IExpressionFunctionCatalog
{
    private static readonly HashSet<string> _keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "AND",
        "OR",
        "NOT",
        "TRUE",
        "FALSE",
        "NULL",
    };

    private readonly Lazy<Catalog> _catalog;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionFunctionCatalog"/> class.
    /// </summary>
    /// <param name="providers">The providers, in the order they were registered.</param>
    public ExpressionFunctionCatalog(IEnumerable<IExpressionFunctionProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        _catalog = new Lazy<Catalog>(() => Build(providers));
    }

    /// <summary>
    /// Gets the catalog of the built-in functions only, for code that runs without services.
    /// </summary>
    public static ExpressionFunctionCatalog BuiltIn { get; } = new([new BuiltInExpressionFunctionProvider()]);

    public IReadOnlyList<ExpressionFunction> Functions => _catalog.Value.Functions;

    public ExpressionFunction Find(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return _catalog.Value.ByName.GetValueOrDefault(name);
    }

    private static Catalog Build(IEnumerable<IExpressionFunctionProvider> providers)
    {
        var byName = new Dictionary<string, ExpressionFunction>(StringComparer.OrdinalIgnoreCase);

        // The built-in functions come first whatever the order of the startups, so that other providers can replace them.
        foreach (var provider in providers.OrderBy(provider => provider is BuiltInExpressionFunctionProvider ? 0 : 1))
        {
            foreach (var function in provider.GetFunctions() ?? [])
            {
                Validate(function, provider);

                // A provider registered later replaces a function of the same name.
                byName[function.Name] = function;
            }
        }

        var functions = byName.Values
            .OrderBy(function => function.Category, StringComparer.Ordinal)
            .ThenBy(function => function.Name, StringComparer.Ordinal)
            .ToArray();

        return new Catalog(functions, byName);
    }

    private static void Validate(ExpressionFunction function, IExpressionFunctionProvider provider)
    {
        var source = provider.GetType().FullName;

        if (function is null)
        {
            throw new InvalidOperationException($"The expression function provider '{source}' returned a null function.");
        }

        // The built-in NOT is the keyword written like a function, listed so that people find it.
        if (!IsValidName(function.Name, allowKeywords: provider is BuiltInExpressionFunctionProvider))
        {
            throw new InvalidOperationException($"The expression function provider '{source}' offers a function named '{function.Name}'. A function name starts with a letter or an underscore, holds only letters, digits and underscores, and can't be AND, OR, NOT, TRUE, FALSE or NULL.");
        }

        if (function.ReturnType is null)
        {
            throw new InvalidOperationException($"The expression function '{function.Name}' of '{source}' has no return type.");
        }

        if (function.IsAggregate ? function.Aggregate is null : function.Invoke is null)
        {
            throw new InvalidOperationException($"The expression function '{function.Name}' of '{source}' has no implementation.");
        }

        if (function.MinArguments < 0 || function.MaxArguments < function.MinArguments)
        {
            throw new InvalidOperationException($"The expression function '{function.Name}' of '{source}' has an invalid number of arguments.");
        }
    }

    private static bool IsValidName(string name, bool allowKeywords)
    {
        if (string.IsNullOrEmpty(name) || (!allowKeywords && _keywords.Contains(name)) || !(char.IsLetter(name[0]) || name[0] == '_'))
        {
            return false;
        }

        foreach (var character in name)
        {
            if (!char.IsLetterOrDigit(character) && character != '_')
            {
                return false;
            }
        }

        return true;
    }

    private sealed record Catalog(IReadOnlyList<ExpressionFunction> Functions, Dictionary<string, ExpressionFunction> ByName);
}
