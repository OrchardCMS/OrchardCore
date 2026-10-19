using OrchardCore.DataSources;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

public sealed class DataFilterOperatorExtensionsTests
{
    [Theory]
    [InlineData(DataFilterOperator.Contains, DataFieldType.Text, true)]
    [InlineData(DataFilterOperator.NotContains, DataFieldType.Integer, false)]
    [InlineData(DataFilterOperator.StartsWith, DataFieldType.DateTime, false)]
    [InlineData(DataFilterOperator.EndsWith, DataFieldType.Boolean, false)]
    [InlineData(DataFilterOperator.GreaterThan, DataFieldType.Decimal, true)]
    [InlineData(DataFilterOperator.LessThanOrEqual, DataFieldType.Date, true)]
    [InlineData(DataFilterOperator.Between, DataFieldType.Integer, true)]
    [InlineData(DataFilterOperator.Between, DataFieldType.Text, false)]
    [InlineData(DataFilterOperator.GreaterThanOrEqual, DataFieldType.Boolean, false)]
    [InlineData(DataFilterOperator.InLastDays, DataFieldType.DateTime, true)]
    [InlineData(DataFilterOperator.InNextDays, DataFieldType.Date, true)]
    [InlineData(DataFilterOperator.InLastDays, DataFieldType.Integer, false)]
    [InlineData(DataFilterOperator.InNextDays, DataFieldType.Text, false)]
    public void AppliesTo_OperatorAndType_ReturnsWhetherItApplies(DataFilterOperator filterOperator, DataFieldType dataType, bool expected)
    {
        // Act
        var applies = filterOperator.AppliesTo(dataType);

        // Assert
        Assert.Equal(expected, applies);
    }

    [Theory]
    [InlineData(DataFilterOperator.Equals)]
    [InlineData(DataFilterOperator.NotEquals)]
    [InlineData(DataFilterOperator.In)]
    [InlineData(DataFilterOperator.NotIn)]
    [InlineData(DataFilterOperator.IsEmpty)]
    [InlineData(DataFilterOperator.IsNotEmpty)]
    public void AppliesTo_EqualityOrEmptiness_AppliesToEveryType(DataFilterOperator filterOperator)
    {
        // Act & Assert
        Assert.All(Enum.GetValues<DataFieldType>(), dataType => Assert.True(filterOperator.AppliesTo(dataType)));
    }
}
