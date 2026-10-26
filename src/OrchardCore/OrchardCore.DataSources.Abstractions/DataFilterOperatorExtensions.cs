namespace OrchardCore.DataSources;

/// <summary>
/// Tells which <see cref="DataFilterOperator"/> applies to which <see cref="DataFieldType"/>, so that editors offer only
/// the comparisons that make sense for a field, and consumers report the others.
/// </summary>
public static class DataFilterOperatorExtensions
{
    /// <summary>
    /// Determines whether an operator applies to the values of a type. <see cref="DataFilterOperator.Equals"/>,
    /// <see cref="DataFilterOperator.NotEquals"/>, <see cref="DataFilterOperator.In"/>,
    /// <see cref="DataFilterOperator.NotIn"/>, <see cref="DataFilterOperator.IsEmpty"/> and
    /// <see cref="DataFilterOperator.IsNotEmpty"/> apply to every type. The text operators, such as
    /// <see cref="DataFilterOperator.Contains"/>, apply to <see cref="DataFieldType.Text"/>. The comparisons, such as
    /// <see cref="DataFilterOperator.GreaterThan"/> and <see cref="DataFilterOperator.Between"/>, apply to numbers and
    /// dates. <see cref="DataFilterOperator.InLastDays"/> and <see cref="DataFilterOperator.InNextDays"/> apply to dates.
    /// </summary>
    /// <param name="filterOperator">The operator.</param>
    /// <param name="dataType">The type of the field the operator compares.</param>
    /// <returns><see langword="true"/> when the operator applies to the type.</returns>
    public static bool AppliesTo(this DataFilterOperator filterOperator, DataFieldType dataType)
    {
        return filterOperator switch
        {
            DataFilterOperator.Contains or
            DataFilterOperator.NotContains or
            DataFilterOperator.StartsWith or
            DataFilterOperator.EndsWith => dataType == DataFieldType.Text,

            DataFilterOperator.GreaterThan or
            DataFilterOperator.GreaterThanOrEqual or
            DataFilterOperator.LessThan or
            DataFilterOperator.LessThanOrEqual or
            DataFilterOperator.Between => DataValues.IsNumeric(dataType) || DataValues.IsTemporal(dataType),

            DataFilterOperator.InLastDays or
            DataFilterOperator.InNextDays => DataValues.IsTemporal(dataType),

            _ => true,
        };
    }
}
