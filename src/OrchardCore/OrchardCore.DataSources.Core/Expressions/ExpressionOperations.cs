using System.Globalization;

namespace OrchardCore.DataSources.Expressions;

/// <summary>
/// Implements the operators and value conversions of formulas. A missing operand yields a missing result,
/// dividing by zero yields a missing result, and integer arithmetic that overflows continues in decimals.
/// </summary>
public static class ExpressionOperations
{
    /// <summary>
    /// Converts a value to a decimal number.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The number, or <see langword="null"/> when the value is not a number.</returns>
    public static decimal? ToNumber(object value)
    {
        return value is decimal number
            ? number
            : (decimal?)DataValues.Coerce(value, DataFieldType.Decimal);
    }

    /// <summary>
    /// Converts a value to a whole number, truncating any fraction.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The whole number, or <see langword="null"/> when the value is not a number.</returns>
    public static long? ToInteger(object value)
    {
        return value is long whole
            ? whole
            : (long?)DataValues.Coerce(value, DataFieldType.Integer);
    }

    /// <summary>
    /// Converts a value to a date and time.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The date and time, or <see langword="null"/> when the value is not a date.</returns>
    public static DateTime? ToDateTime(object value)
    {
        return value is DateTime date
            ? date
            : (DateTime?)DataValues.Coerce(value, DataFieldType.DateTime);
    }

    /// <summary>
    /// Converts a value to text.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The text, or <see langword="null"/> when the value is missing.</returns>
    public static string ToText(object value)
    {
        return DataValues.ToText(value);
    }

    /// <summary>
    /// Determines whether a value counts as true: <see langword="true"/>, a non-zero number, or a non-empty text other
    /// than <c>false</c> or <c>0</c>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The truth of the value; a missing value is false.</returns>
    public static bool IsTrue(object value)
    {
        return value switch
        {
            null => false,
            bool flag => flag,
            string text => (bool?)DataValues.Coerce(text, DataFieldType.Boolean) ?? text.Length > 0,
            DateTime => true,
            _ => ToNumber(value) is decimal number && number != 0,
        };
    }

    /// <summary>
    /// Applies a binary operator.
    /// </summary>
    /// <param name="op">The normalized operator.</param>
    /// <param name="left">The left value.</param>
    /// <param name="right">The right value.</param>
    /// <returns>The result.</returns>
    public static object Apply(string op, object left, object right)
    {
        return op switch
        {
            "+" => Add(left, right),
            "-" => Subtract(left, right),
            "*" => Multiply(left, right),
            "/" => Divide(left, right),
            "%" => Modulo(left, right),
            "&" => string.Concat(ToText(left), ToText(right)),
            "=" => AreEqual(left, right),
            "!=" => !AreEqual(left, right),
            "<" => CompareNonMissing(left, right, comparison => comparison < 0),
            "<=" => CompareNonMissing(left, right, comparison => comparison <= 0),
            ">" => CompareNonMissing(left, right, comparison => comparison > 0),
            ">=" => CompareNonMissing(left, right, comparison => comparison >= 0),
            "AND" => IsTrue(left) && IsTrue(right),
            "OR" => IsTrue(left) || IsTrue(right),
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, null),
        };
    }

    /// <summary>
    /// Negates a number.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The negated number, or <see langword="null"/> when the value is not a number.</returns>
    public static object Negate(object value)
    {
        if (value is long whole && whole != long.MinValue)
        {
            return -whole;
        }

        return -ToNumber(value);
    }

    /// <summary>
    /// Narrows a decimal to a whole number when it has no fraction and fits.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="long"/> when possible; otherwise the decimal.</returns>
    public static object NarrowToInteger(decimal value)
    {
        if (decimal.Truncate(value) == value && value >= long.MinValue && value <= long.MaxValue)
        {
            return (long)value;
        }

        return value;
    }

    /// <summary>
    /// Formats a value with a .NET format string and the invariant culture.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The format string.</param>
    /// <returns>The formatted text.</returns>
    public static string Format(object value, string format)
    {
        if (value is null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(format) || value is not IFormattable formattable)
        {
            return ToText(value);
        }

        try
        {
            return formattable.ToString(format, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            return ToText(value);
        }
    }

    private static bool AreEqual(object left, object right)
    {
        if (left is null || right is null)
        {
            return DataValues.IsEmpty(left) && DataValues.IsEmpty(right);
        }

        if (left is DateTime && right is string)
        {
            right = ToDateTime(right);
        }
        else if (right is DateTime && left is string)
        {
            left = ToDateTime(left);
        }

        return DataValues.AreEqual(left, right);
    }

    private static bool CompareNonMissing(object left, object right, Func<int, bool> predicate)
    {
        if (left is null || right is null)
        {
            return false;
        }

        if (left is DateTime && right is string)
        {
            right = ToDateTime(right);
        }
        else if (right is DateTime && left is string)
        {
            left = ToDateTime(left);
        }

        if (left is null || right is null)
        {
            return false;
        }

        return predicate(DataValues.Compare(left, right));
    }

    private static object Add(object left, object right)
    {
        if (left is null || right is null)
        {
            return null;
        }

        if (left is string || right is string)
        {
            return string.Concat(ToText(left), ToText(right));
        }

        if (left is DateTime date)
        {
            return ToNumber(right) is decimal days ? AddDays(date, days) : null;
        }

        if (right is DateTime rightDate)
        {
            return ToNumber(left) is decimal days ? AddDays(rightDate, days) : null;
        }

        if (left is long a && right is long b)
        {
            try
            {
                return checked(a + b);
            }
            catch (OverflowException)
            {
                return (decimal)a + b;
            }
        }

        return Decimal(left, right, (x, y) => x + y);
    }

    private static object Subtract(object left, object right)
    {
        if (left is null || right is null)
        {
            return null;
        }

        if (left is DateTime leftDate && right is DateTime rightDate)
        {
            return (decimal)(leftDate - rightDate).TotalDays;
        }

        if (left is DateTime date)
        {
            return ToNumber(right) is decimal days ? AddDays(date, -days) : null;
        }

        if (left is long a && right is long b)
        {
            try
            {
                return checked(a - b);
            }
            catch (OverflowException)
            {
                return (decimal)a - b;
            }
        }

        return Decimal(left, right, (x, y) => x - y);
    }

    private static object Multiply(object left, object right)
    {
        if (left is long a && right is long b)
        {
            try
            {
                return checked(a * b);
            }
            catch (OverflowException)
            {
                return Decimal(left, right, (x, y) => x * y);
            }
        }

        return Decimal(left, right, (x, y) => x * y);
    }

    private static object Divide(object left, object right)
    {
        return Decimal(left, right, (x, y) => y == 0 ? null : x / y);
    }

    private static object Modulo(object left, object right)
    {
        if (left is long a && right is long b)
        {
            return b == 0 ? null : a % b;
        }

        return Decimal(left, right, (x, y) => y == 0 ? null : x % y);
    }

    private static object Decimal(object left, object right, Func<decimal, decimal, decimal?> operation)
    {
        if (ToNumber(left) is not decimal x || ToNumber(right) is not decimal y)
        {
            return null;
        }

        try
        {
            object result = operation(x, y);

            return result;
        }
        catch (OverflowException)
        {
            return null;
        }
    }

    private static DateTime? AddDays(DateTime date, decimal days)
    {
        try
        {
            return date.AddDays((double)days);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
