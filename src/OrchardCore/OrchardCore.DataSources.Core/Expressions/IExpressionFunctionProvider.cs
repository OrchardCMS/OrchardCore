namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// Offers functions to formulas. Register an implementation as a singleton with
/// <c>services.AddExpressionFunctionProvider&lt;TProvider&gt;()</c>; its functions are added to the
/// <see cref="IExpressionFunctionCatalog"/> once, the first time the catalog is used.
/// </summary>
public interface IExpressionFunctionProvider
{
    /// <summary>
    /// Gets the functions the provider offers. A function replaces a built-in function of the same name, and the one
    /// of the same name offered by a provider registered before.
    /// </summary>
    /// <returns>The functions.</returns>
    IEnumerable<ExpressionFunction> GetFunctions();
}
