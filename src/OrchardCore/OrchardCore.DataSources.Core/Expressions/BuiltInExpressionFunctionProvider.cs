namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// Offers the built-in functions of <see cref="ExpressionFunctions"/>: text, number, date, logical and aggregate
/// functions.
/// </summary>
public sealed class BuiltInExpressionFunctionProvider : IExpressionFunctionProvider
{
    public IEnumerable<ExpressionFunction> GetFunctions() => ExpressionFunctions.All;
}
