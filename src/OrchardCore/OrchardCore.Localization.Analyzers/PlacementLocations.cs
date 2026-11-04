using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace OrchardCore.Localization.Analyzers;

/// <summary>
/// The kind of placement value that a call sets.
/// </summary>
internal enum PlacementValueKind
{
    /// <summary>A placement location, such as <c>Content:5#Settings</c>.</summary>
    Location,

    /// <summary>A tab name.</summary>
    Tab,

    /// <summary>A card name.</summary>
    Card,
}

/// <summary>
/// Finds the calls that set a placement location or a tab or card name, and the values they can set, for
/// <see cref="PlacementCatalogGenerator"/> and <see cref="PlacementLocationAnalyzer"/>.
/// </summary>
internal sealed class PlacementLocations
{
    public const string ShapeResultMetadataName = "OrchardCore.DisplayManagement.Views.ShapeResult";
    public const string LocationBuilderMetadataName = "OrchardCore.DisplayManagement.Descriptors.PlacementLocationBuilder";
    public const string LocationAttributeMetadataName = "OrchardCore.DisplayManagement.Descriptors.PlacementLocationAttribute";
    public const string CatalogAttributeMetadataName = "OrchardCore.DisplayManagement.Descriptors.PlacementCatalogAttribute";

    private readonly INamedTypeSymbol? _shapeResult;
    private readonly INamedTypeSymbol? _locationBuilder;

    private PlacementLocations(INamedTypeSymbol? shapeResult, INamedTypeSymbol? locationBuilder)
    {
        _shapeResult = shapeResult;
        _locationBuilder = locationBuilder;
    }

    /// <summary>
    /// Gets the placement types of a compilation, or <see langword="null"/> when it doesn't reference them.
    /// </summary>
    public static PlacementLocations? Create(Compilation compilation)
    {
        var shapeResult = compilation.GetTypeByMetadataName(ShapeResultMetadataName);
        var locationBuilder = compilation.GetTypeByMetadataName(LocationBuilderMetadataName);

        return shapeResult is null && locationBuilder is null ? null : new PlacementLocations(shapeResult, locationBuilder);
    }

    /// <summary>
    /// Gets the argument of a <c>ShapeResult.Location()</c> call that holds the location, or of a
    /// <c>PlacementLocationBuilder.Tab()</c> or <c>Card()</c> call that holds the name.
    /// </summary>
    public bool TryGetArgument(IInvocationOperation invocation, out IArgumentOperation? argument, out PlacementValueKind kind)
    {
        argument = null;
        kind = PlacementValueKind.Location;

        var method = invocation.TargetMethod;
        string parameterName;

        if (method.Name == "Location" && SymbolEqualityComparer.Default.Equals(method.ContainingType, _shapeResult))
        {
            parameterName = "location";
        }
        else if (method.Name is "Tab" or "Card" && SymbolEqualityComparer.Default.Equals(method.ContainingType, _locationBuilder))
        {
            parameterName = "name";
            kind = method.Name == "Tab" ? PlacementValueKind.Tab : PlacementValueKind.Card;
        }
        else
        {
            return false;
        }

        // The overloads that take a PlacementInfo or a builder action have no location to read.
        argument = invocation.Arguments.FirstOrDefault(argument =>
            argument.Parameter?.Name == parameterName &&
            argument.Parameter.Type.SpecialType == SpecialType.System_String);

        return argument is not null;
    }

    /// <summary>
    /// Adds the values that an expression can have to <paramref name="values"/>: a compile-time constant, or a
    /// conditional, coalesce or switch expression of constants. A concatenation or an interpolation that can have no
    /// tab or card, such as a computed position, has no value to add. Returns <see langword="false"/> otherwise.
    /// </summary>
    public static bool TryGetConstantValues(IOperation operation, List<string?> values)
    {
        operation = Unwrap(operation);

        if (operation.ConstantValue.HasValue)
        {
            values.Add(operation.ConstantValue.Value as string);

            return true;
        }

        return operation switch
        {
            IConditionalOperation { WhenFalse: not null } conditional =>
                TryGetConstantValues(conditional.WhenTrue, values) && TryGetConstantValues(conditional.WhenFalse, values),
            ICoalesceOperation coalesce =>
                TryGetConstantValues(coalesce.Value, values) && TryGetConstantValues(coalesce.WhenNull, values),
            ISwitchExpressionOperation switchExpression =>
                switchExpression.Arms.All(arm => TryGetConstantValues(arm.Value, values)),
            IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } or IInterpolatedStringOperation => HasNoTabOrCard(operation),
            _ => false,
        };
    }

    /// <summary>
    /// Gets whether a location is built only from text without a tab or card delimiter and from numbers.
    /// </summary>
    private static bool HasNoTabOrCard(IOperation operation)
    {
        operation = Unwrap(operation);

        if (operation.ConstantValue.HasValue)
        {
            return operation.ConstantValue.Value is not string text || !HasTabOrCard(text);
        }

        return operation switch
        {
            IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } concatenation =>
                HasNoTabOrCard(concatenation.LeftOperand) && HasNoTabOrCard(concatenation.RightOperand),
            IInterpolatedStringOperation interpolation => interpolation.Parts.All(part => part switch
            {
                IInterpolatedStringTextOperation text => HasNoTabOrCard(text.Text),
                IInterpolationOperation hole => HasNoTabOrCard(hole.Expression),
                _ => false,
            }),
            _ => operation.Type?.SpecialType is >= SpecialType.System_SByte and <= SpecialType.System_Double,
        };
    }

    private static IOperation Unwrap(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }

    /// <summary>
    /// Gets whether a location has a tab or a card.
    /// </summary>
    public static bool HasTabOrCard(string location)
        => location.IndexOf('#') >= 0 || location.IndexOf('%') >= 0;
}
