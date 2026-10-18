namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// The functions formulas can call: the built-in functions and those the registered
/// <see cref="IExpressionFunctionProvider"/> implementations offer. Pass it to
/// <see cref="ExpressionCompiler.Compile(string, IExpressionScope, IExpressionFunctionCatalog, Microsoft.Extensions.Localization.IStringLocalizer)"/>,
/// and list its functions to the people who write formulas.
/// </summary>
public interface IExpressionFunctionCatalog
{
    /// <summary>
    /// Gets every function, ordered by category and name.
    /// </summary>
    IReadOnlyList<ExpressionFunction> Functions { get; }

    /// <summary>
    /// Finds a function by name, ignoring case.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <returns>The function, or <see langword="null"/> when there is none.</returns>
    ExpressionFunction Find(string name);
}
