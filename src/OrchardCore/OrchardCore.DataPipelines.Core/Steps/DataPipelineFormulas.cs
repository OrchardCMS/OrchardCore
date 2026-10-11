using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Expressions;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Compiles and evaluates the formulas of steps, such as the condition of a filter or the formula of a calculated
/// field. A formula refers to a field by its name in square brackets, such as <c>[Price] * [Quantity]</c>.
/// </summary>
public static class DataPipelineFormulas
{
    /// <summary>
    /// Compiles a formula against the fields of a step's input.
    /// </summary>
    /// <param name="formula">The formula.</param>
    /// <param name="fields">The fields the formula can refer to.</param>
    /// <param name="localizer">Localizes the errors.</param>
    /// <param name="error">The reason the formula is invalid, if it is.</param>
    /// <returns>The compiled formula, or <see langword="null"/> when it is invalid.</returns>
    public static CompiledExpression TryCompile(string formula, IReadOnlyList<DataField> fields, IStringLocalizer localizer, out string error)
    {
        ArgumentNullException.ThrowIfNull(localizer);

        error = null;

        if (string.IsNullOrWhiteSpace(formula))
        {
            error = localizer["Enter a formula."];

            return null;
        }

        try
        {
            var compiled = ExpressionCompiler.Compile(formula, new FieldListExpressionScope(fields ?? []), localizer);

            if (compiled.IsAggregate)
            {
                error = localizer["A formula of a step reads one row at a time, so it can't use an aggregate function such as SUM. Use a Group and summarize step instead."];

                return null;
            }

            return compiled;
        }
        catch (ExpressionException ex)
        {
            error = ex.Message;

            return null;
        }
    }

    /// <summary>
    /// Gets the date and time the formulas and file names of a run use: when the run started, in the time zone of the
    /// site, so that <c>NOW()</c> is the same for every row and <c>TODAY()</c> is the site's current day.
    /// </summary>
    /// <param name="run">The run.</param>
    /// <returns>The local date and time the run started.</returns>
    public static async Task<DateTime> GetRunTimeAsync(DataPipelineRunContext run)
    {
        ArgumentNullException.ThrowIfNull(run);

        var clock = run.Services?.GetService<ILocalClock>();
        var started = DateTime.SpecifyKind(run.StartedUtc, DateTimeKind.Utc);

        if (clock is null)
        {
            return started;
        }

        return (await clock.ConvertToLocalAsync(new DateTimeOffset(started))).DateTime;
    }

    /// <summary>
    /// Gets the engine limits.
    /// </summary>
    /// <param name="services">The services of the run.</param>
    /// <returns>The options.</returns>
    public static DataPipelineOptions GetOptions(IServiceProvider services)
        => services?.GetService<IOptions<DataPipelineOptions>>()?.Value ?? new DataPipelineOptions();

    /// <summary>
    /// Evaluates a formula on a row. A row whose values make the formula fail, such as a division by zero,
    /// evaluates to <see langword="null"/>.
    /// </summary>
    /// <param name="expression">The compiled formula.</param>
    /// <param name="row">The row.</param>
    /// <param name="now">The current date and time.</param>
    /// <param name="error">The reason the evaluation failed, if it did.</param>
    /// <returns>The value.</returns>
    public static object Evaluate(CompiledExpression expression, object[] row, DateTime now, out string error)
    {
        ArgumentNullException.ThrowIfNull(expression);

        error = null;

        try
        {
            return expression.Evaluate(new ExpressionContext { Row = row, Now = now });
        }
        catch (Exception ex) when (ex is ExpressionException or ArithmeticException or InvalidCastException or FormatException or ArgumentException)
        {
            error = ex.Message;

            return null;
        }
    }
}
