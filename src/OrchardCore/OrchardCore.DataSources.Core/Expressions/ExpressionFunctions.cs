using System.Globalization;
using OrchardCore.DataSources.Operations;

namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// The catalog of functions formulas can call. Scalar functions return a missing value instead of failing when
/// an argument is missing or of the wrong type.
/// </summary>
public static partial class ExpressionFunctions
{
    /// <summary>
    /// The category of text functions.
    /// </summary>
    public const string TextCategory = "Text";

    /// <summary>
    /// The category of number functions.
    /// </summary>
    public const string NumberCategory = "Number";

    /// <summary>
    /// The category of date functions.
    /// </summary>
    public const string DateCategory = "Date";

    /// <summary>
    /// The category of logical functions.
    /// </summary>
    public const string LogicalCategory = "Logical";

    /// <summary>
    /// The category of aggregate functions.
    /// </summary>
    public const string AggregateCategory = "Aggregate";

    private static readonly Dictionary<string, ExpressionFunction> _functions = Build();

    /// <summary>
    /// Gets every function, ordered by category and name.
    /// </summary>
    public static IReadOnlyList<ExpressionFunction> All { get; } = _functions.Values
        .OrderBy(function => function.Category, StringComparer.Ordinal)
        .ThenBy(function => function.Name, StringComparer.Ordinal)
        .ToArray();

    /// <summary>
    /// Finds a function by name, ignoring case.
    /// </summary>
    /// <param name="name">The function name.</param>
    /// <returns>The function, or <see langword="null"/> when there is none.</returns>
    public static ExpressionFunction Find(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return _functions.GetValueOrDefault(name.ToUpperInvariant());
    }

    /// <summary>
    /// Finds the type that can hold values of every given type: the type itself when all agree, a decimal for mixed
    /// numbers, a date-time for mixed dates, and text otherwise.
    /// </summary>
    /// <param name="types">The types.</param>
    /// <returns>The common type.</returns>
    public static DataFieldType CommonType(IEnumerable<DataFieldType> types)
    {
        var distinct = types.Distinct().ToList();

        if (distinct.Count == 0)
        {
            return DataFieldType.Text;
        }

        if (distinct.Count == 1)
        {
            return distinct[0];
        }

        if (distinct.All(DataValues.IsNumeric))
        {
            return DataFieldType.Decimal;
        }

        if (distinct.All(DataValues.IsTemporal))
        {
            return DataFieldType.DateTime;
        }

        return DataFieldType.Text;
    }

    private static Dictionary<string, ExpressionFunction> Build()
    {
        var functions = new List<ExpressionFunction>();

        AddTextFunctions(functions);
        AddNumberFunctions(functions);
        AddLogicalFunctions(functions);
        AddDateFunctions(functions);
        AddAggregateFunctions(functions);

        return functions.ToDictionary(function => function.Name, StringComparer.Ordinal);
    }

    private static void AddTextFunctions(List<ExpressionFunction> functions)
    {
        functions.Add(Scalar("UPPER", TextCategory, "UPPER(text)", "Converts text to upper case.", 1, 1, Returns(DataFieldType.Text), args => Text(args[0])?.ToUpperInvariant()));
        functions.Add(Scalar("LOWER", TextCategory, "LOWER(text)", "Converts text to lower case.", 1, 1, Returns(DataFieldType.Text), args => Text(args[0])?.ToLowerInvariant()));
        functions.Add(Scalar("TRIM", TextCategory, "TRIM(text)", "Removes leading and trailing spaces.", 1, 1, Returns(DataFieldType.Text), args => Text(args[0])?.Trim()));
        functions.Add(Scalar("PROPER", TextCategory, "PROPER(text)", "Capitalizes the first letter of each word.", 1, 1, Returns(DataFieldType.Text), args => Text(args[0]) is string text ? CultureInfo.InvariantCulture.TextInfo.ToTitleCase(text.ToLowerInvariant()) : null));
        functions.Add(Scalar("LEN", TextCategory, "LEN(text)", "Returns the number of characters in the text.", 1, 1, Returns(DataFieldType.Integer), args => Text(args[0]) is string text ? (long)text.Length : null));
        functions.Add(Scalar("LEFT", TextCategory, "LEFT(text, count)", "Returns the first characters of the text.", 2, 2, Returns(DataFieldType.Text), args => Left(Text(args[0]), Whole(args[1]))));
        functions.Add(Scalar("RIGHT", TextCategory, "RIGHT(text, count)", "Returns the last characters of the text.", 2, 2, Returns(DataFieldType.Text), args => Right(Text(args[0]), Whole(args[1]))));
        functions.Add(Scalar("MID", TextCategory, "MID(text, start, [count])", "Returns characters from the middle of the text. The first character is at position 1.", 2, 3, Returns(DataFieldType.Text), args => Mid(Text(args[0]), Whole(args[1]), args.Length > 2 ? Whole(args[2]) : null)));
        functions.Add(Scalar("SUBSTRING", TextCategory, "SUBSTRING(text, start, [count])", "Same as MID.", 2, 3, Returns(DataFieldType.Text), args => Mid(Text(args[0]), Whole(args[1]), args.Length > 2 ? Whole(args[2]) : null)));
        functions.Add(Scalar("REPLACE", TextCategory, "REPLACE(text, find, replacement)", "Replaces every occurrence of a text, ignoring case.", 3, 3, Returns(DataFieldType.Text), args => Replace(Text(args[0]), Text(args[1]), Text(args[2]))));
        functions.Add(Scalar("CONCAT", TextCategory, "CONCAT(value1, value2, ...)", "Joins values into one text. Missing values are skipped.", 1, int.MaxValue, Returns(DataFieldType.Text), args => string.Concat(args.Select(Text))));
        functions.Add(Scalar("FIND", TextCategory, "FIND(text, search)", "Returns the position of the search text (starting at 1), or 0 when it is not found.", 2, 2, Returns(DataFieldType.Integer), args => Text(args[0]) is string text && Text(args[1]) is string search ? (long)(text.IndexOf(search, StringComparison.OrdinalIgnoreCase) + 1) : null));
        functions.Add(Scalar("CONTAINS", TextCategory, "CONTAINS(text, search)", "Returns true when the text contains the search text, ignoring case.", 2, 2, Returns(DataFieldType.Boolean), args => Text(args[0]) is string text && Text(args[1]) is string search && text.Contains(search, StringComparison.OrdinalIgnoreCase)));
        functions.Add(Scalar("STARTSWITH", TextCategory, "STARTSWITH(text, search)", "Returns true when the text starts with the search text, ignoring case.", 2, 2, Returns(DataFieldType.Boolean), args => Text(args[0]) is string text && Text(args[1]) is string search && text.StartsWith(search, StringComparison.OrdinalIgnoreCase)));
        functions.Add(Scalar("ENDSWITH", TextCategory, "ENDSWITH(text, search)", "Returns true when the text ends with the search text, ignoring case.", 2, 2, Returns(DataFieldType.Boolean), args => Text(args[0]) is string text && Text(args[1]) is string search && text.EndsWith(search, StringComparison.OrdinalIgnoreCase)));
        functions.Add(Scalar("SPLIT", TextCategory, "SPLIT(text, separator, index)", "Splits the text and returns the part at the index (starting at 1).", 3, 3, Returns(DataFieldType.Text), args => Split(Text(args[0]), Text(args[1]), Whole(args[2]))));
        functions.Add(Scalar("TEXT", TextCategory, "TEXT(value, [format])", "Converts a value to text, optionally with a .NET format such as 'N2' or 'yyyy-MM'.", 1, 2, Returns(DataFieldType.Text), args => ExpressionOperations.Format(args[0], args.Length > 1 ? Text(args[1]) : null)));
    }

    private static void AddNumberFunctions(List<ExpressionFunction> functions)
    {
        functions.Add(Scalar("ABS", NumberCategory, "ABS(number)", "Returns the absolute value of a number.", 1, 1, types => Numeric(types[0]), Abs));
        functions.Add(Scalar("ROUND", NumberCategory, "ROUND(number, [decimals])", "Rounds a number to a number of decimals (0 by default).", 1, 2, types => types.Count > 1 ? DataFieldType.Decimal : DataFieldType.Integer, Round));
        functions.Add(Scalar("FLOOR", NumberCategory, "FLOOR(number)", "Rounds a number down to a whole number.", 1, 1, Returns(DataFieldType.Integer), args => Number(args[0]) is decimal number ? ExpressionOperations.NarrowToInteger(Math.Floor(number)) : null));
        functions.Add(Scalar("CEILING", NumberCategory, "CEILING(number)", "Rounds a number up to a whole number.", 1, 1, Returns(DataFieldType.Integer), args => Number(args[0]) is decimal number ? ExpressionOperations.NarrowToInteger(Math.Ceiling(number)) : null));
        functions.Add(Scalar("POWER", NumberCategory, "POWER(number, exponent)", "Raises a number to a power.", 2, 2, Returns(DataFieldType.Decimal), args => Double(args[0], args[1], Math.Pow)));
        functions.Add(Scalar("SQRT", NumberCategory, "SQRT(number)", "Returns the square root of a number.", 1, 1, Returns(DataFieldType.Decimal), args => Number(args[0]) is decimal number && number >= 0 ? (decimal)Math.Sqrt((double)number) : null));
        functions.Add(Scalar("MOD", NumberCategory, "MOD(number, divisor)", "Returns the remainder of a division.", 2, 2, types => CommonType(types.Select(Numeric)), args => ExpressionOperations.Apply("%", args[0], args[1])));
        functions.Add(Scalar("SIGN", NumberCategory, "SIGN(number)", "Returns -1, 0, or 1 for a negative, zero, or positive number.", 1, 1, Returns(DataFieldType.Integer), args => Number(args[0]) is decimal number ? (long)Math.Sign(number) : null));
        functions.Add(Scalar("NUMBER", NumberCategory, "NUMBER(value)", "Converts a value to a decimal number.", 1, 1, Returns(DataFieldType.Decimal), args => Number(args[0])));
        functions.Add(Scalar("INTEGER", NumberCategory, "INTEGER(value)", "Converts a value to a whole number, dropping any fraction.", 1, 1, Returns(DataFieldType.Integer), args => ExpressionOperations.ToInteger(args[0])));
        functions.Add(Scalar("LEAST", NumberCategory, "LEAST(value1, value2, ...)", "Returns the smallest of the values.", 1, int.MaxValue, types => CommonType(types), args => DataAggregations.Min(args)));
        functions.Add(Scalar("GREATEST", NumberCategory, "GREATEST(value1, value2, ...)", "Returns the largest of the values.", 1, int.MaxValue, types => CommonType(types), args => DataAggregations.Max(args)));
    }

    private static void AddLogicalFunctions(List<ExpressionFunction> functions)
    {
        functions.Add(Scalar("IF", LogicalCategory, "IF(condition, then, [else])", "Returns one value when the condition is true and another when it is not.", 2, 3, types => CommonType(types.Skip(1)), args => ExpressionOperations.IsTrue(args[0]) ? args[1] : args.Length > 2 ? args[2] : null));
        functions.Add(Scalar("IIF", LogicalCategory, "IIF(condition, then, else)", "Same as IF.", 2, 3, types => CommonType(types.Skip(1)), args => ExpressionOperations.IsTrue(args[0]) ? args[1] : args.Length > 2 ? args[2] : null));
        functions.Add(Scalar("IFS", LogicalCategory, "IFS(condition1, value1, condition2, value2, ..., [else])", "Returns the value of the first condition that is true.", 2, int.MaxValue, types => CommonType(types.Where((_, index) => index % 2 == 1 || index == types.Count - 1)), Ifs));
        functions.Add(Scalar("SWITCH", LogicalCategory, "SWITCH(value, match1, result1, match2, result2, ..., [else])", "Returns the result paired with the first match of the value.", 3, int.MaxValue, types => CommonType(types.Where((_, index) => (index > 0 && index % 2 == 0) || (index == types.Count - 1 && types.Count % 2 == 0))), Switch));
        functions.Add(Scalar("COALESCE", LogicalCategory, "COALESCE(value1, value2, ...)", "Returns the first value that is not missing.", 1, int.MaxValue, types => CommonType(types), args => args.FirstOrDefault(arg => !DataValues.IsEmpty(arg))));
        functions.Add(Scalar("IFNULL", LogicalCategory, "IFNULL(value, fallback)", "Returns the fallback when the value is missing.", 2, 2, types => CommonType(types), args => DataValues.IsEmpty(args[0]) ? args[1] : args[0]));
        functions.Add(Scalar("ISNULL", LogicalCategory, "ISNULL(value)", "Returns true when the value is missing.", 1, 1, Returns(DataFieldType.Boolean), args => args[0] is null));
        functions.Add(Scalar("ISBLANK", LogicalCategory, "ISBLANK(value)", "Returns true when the value is missing or empty text.", 1, 1, Returns(DataFieldType.Boolean), args => DataValues.IsEmpty(args[0])));
        functions.Add(Scalar("IN", LogicalCategory, "IN(value, option1, option2, ...)", "Returns true when the value equals any option.", 2, int.MaxValue, Returns(DataFieldType.Boolean), args => args.Skip(1).Any(option => ExpressionOperations.Apply("=", args[0], option) is true)));
        functions.Add(Scalar("NOT", LogicalCategory, "NOT(condition)", "Returns true when the condition is not true.", 1, 1, Returns(DataFieldType.Boolean), args => !ExpressionOperations.IsTrue(args[0])));
    }

    private static void AddAggregateFunctions(List<ExpressionFunction> functions)
    {
        functions.Add(Aggregated("SUM", "SUM(number)", "Adds the values of the group.", true, types => types.Count == 1 && types[0] == DataFieldType.Integer ? DataFieldType.Integer : DataFieldType.Decimal, DataAggregations.Sum));
        functions.Add(Aggregated("AVG", "AVG(number)", "Averages the values of the group.", true, Returns(DataFieldType.Decimal), DataAggregations.Average));
        functions.Add(Aggregated("AVERAGE", "AVERAGE(number)", "Same as AVG.", true, Returns(DataFieldType.Decimal), DataAggregations.Average));
        functions.Add(Aggregated("MEDIAN", "MEDIAN(number)", "Returns the middle value of the group.", true, Returns(DataFieldType.Decimal), DataAggregations.Median));
        functions.Add(Aggregated("MIN", "MIN(value)", "Returns the smallest value of the group.", false, types => types.Count == 1 ? types[0] : DataFieldType.Text, DataAggregations.Min));
        functions.Add(Aggregated("MAX", "MAX(value)", "Returns the largest value of the group.", false, types => types.Count == 1 ? types[0] : DataFieldType.Text, DataAggregations.Max));
        functions.Add(Aggregated("COUNTD", "COUNTD(value)", "Counts the distinct values of the group.", false, Returns(DataFieldType.Integer), values => DataAggregations.CountDistinct(values)));

        var count = Aggregated("COUNT", "COUNT([value])", "Counts the rows of the group, or the rows where the value is present.", false, Returns(DataFieldType.Integer), values => DataAggregations.Count(values));

        functions.Add(new ExpressionFunction
        {
            Name = count.Name,
            Category = count.Category,
            Signature = count.Signature,
            Description = count.Description,
            MinArguments = 0,
            MaxArguments = 1,
            IsAggregate = true,
            ReturnType = count.ReturnType,
            Aggregate = count.Aggregate,
        });
    }

    private static ExpressionFunction Scalar(
        string name,
        string category,
        string signature,
        string description,
        int minArguments,
        int maxArguments,
        Func<IReadOnlyList<DataFieldType>, DataFieldType> returnType,
        Func<object[], object> invoke)
    {
        return new ExpressionFunction
        {
            Name = name,
            Category = category,
            Signature = signature,
            Description = description,
            MinArguments = minArguments,
            MaxArguments = maxArguments,
            ReturnType = returnType,
            Invoke = (args, _) => invoke(args),
        };
    }

    private static ExpressionFunction Aggregated(
        string name,
        string signature,
        string description,
        bool requiresNumericArgument,
        Func<IReadOnlyList<DataFieldType>, DataFieldType> returnType,
        Func<IReadOnlyList<object>, object> aggregate)
    {
        return new ExpressionFunction
        {
            Name = name,
            Category = AggregateCategory,
            Signature = signature,
            Description = description,
            MinArguments = 1,
            MaxArguments = 1,
            IsAggregate = true,
            RequiresNumericArgument = requiresNumericArgument,
            ReturnType = returnType,
            Aggregate = aggregate,
        };
    }

    private static Func<IReadOnlyList<DataFieldType>, DataFieldType> Returns(DataFieldType dataType)
    {
        return _ => dataType;
    }

    private static DataFieldType Numeric(DataFieldType dataType)
    {
        return dataType == DataFieldType.Integer ? DataFieldType.Integer : DataFieldType.Decimal;
    }

    private static string Text(object value)
    {
        return ExpressionOperations.ToText(value);
    }

    private static decimal? Number(object value)
    {
        return ExpressionOperations.ToNumber(value);
    }

    private static int? Whole(object value)
    {
        return ExpressionOperations.ToInteger(value) is long whole && whole >= int.MinValue && whole <= int.MaxValue
            ? (int)whole
            : null;
    }

    private static decimal? Double(object left, object right, Func<double, double, double> operation)
    {
        if (Number(left) is not decimal x || Number(right) is not decimal y)
        {
            return null;
        }

        var result = operation((double)x, (double)y);

        if (!double.IsFinite(result) || Math.Abs(result) > (double)decimal.MaxValue)
        {
            return null;
        }

        return (decimal)result;
    }

    private static object Abs(object[] args)
    {
        if (args[0] is long whole && whole != long.MinValue)
        {
            return Math.Abs(whole);
        }

        return Number(args[0]) is decimal number ? Math.Abs(number) : null;
    }

    private static object Round(object[] args)
    {
        if (Number(args[0]) is not decimal number)
        {
            return null;
        }

        if (args.Length < 2)
        {
            return ExpressionOperations.NarrowToInteger(Math.Round(number, MidpointRounding.AwayFromZero));
        }

        var decimals = Math.Clamp(Whole(args[1]) ?? 0, 0, 28);

        return Math.Round(number, decimals, MidpointRounding.AwayFromZero);
    }

    private static string Left(string text, int? count)
    {
        if (text is null || count is null)
        {
            return null;
        }

        return text[..Math.Clamp(count.Value, 0, text.Length)];
    }

    private static string Right(string text, int? count)
    {
        if (text is null || count is null)
        {
            return null;
        }

        var length = Math.Clamp(count.Value, 0, text.Length);

        return text[(text.Length - length)..];
    }

    private static string Mid(string text, int? start, int? count)
    {
        if (text is null || start is null)
        {
            return null;
        }

        var from = Math.Clamp(start.Value - 1, 0, text.Length);
        var length = Math.Clamp(count ?? text.Length, 0, text.Length - from);

        return text.Substring(from, length);
    }

    private static string Replace(string text, string find, string replacement)
    {
        if (text is null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(find))
        {
            return text;
        }

        return text.Replace(find, replacement ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static string Split(string text, string separator, int? index)
    {
        if (text is null || string.IsNullOrEmpty(separator) || index is null || index.Value < 1)
        {
            return null;
        }

        var parts = text.Split(separator);

        return index.Value <= parts.Length ? parts[index.Value - 1] : null;
    }

    private static object Ifs(object[] args)
    {
        for (var index = 0; index + 1 < args.Length; index += 2)
        {
            if (ExpressionOperations.IsTrue(args[index]))
            {
                return args[index + 1];
            }
        }

        return args.Length % 2 == 1 ? args[^1] : null;
    }

    private static object Switch(object[] args)
    {
        for (var index = 1; index + 1 < args.Length; index += 2)
        {
            if (ExpressionOperations.Apply("=", args[0], args[index]) is true)
            {
                return args[index + 1];
            }
        }

        return args.Length % 2 == 0 ? args[^1] : null;
    }
}
