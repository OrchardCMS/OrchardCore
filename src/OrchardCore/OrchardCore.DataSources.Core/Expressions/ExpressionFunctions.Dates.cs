using System.Globalization;

namespace OrchardCore.DataSources.Expressions;

public static partial class ExpressionFunctions
{
    private static void AddDateFunctions(List<ExpressionFunction> functions)
    {
        functions.Add(new ExpressionFunction
        {
            Name = "NOW",
            Category = DateCategory,
            Signature = "NOW()",
            Description = "Returns the current date and time.",
            MinArguments = 0,
            MaxArguments = 0,
            ReturnType = Returns(DataFieldType.DateTime),
            Invoke = (_, context) => context.Now,
        });

        functions.Add(new ExpressionFunction
        {
            Name = "TODAY",
            Category = DateCategory,
            Signature = "TODAY()",
            Description = "Returns the current date.",
            MinArguments = 0,
            MaxArguments = 0,
            ReturnType = Returns(DataFieldType.Date),
            Invoke = (_, context) => context.Now.Date,
        });

        functions.Add(Scalar("YEAR", DateCategory, "YEAR(date)", "Returns the year of a date.", 1, 1, Returns(DataFieldType.Integer), args => Date(args[0]) is DateTime date ? (long)date.Year : null));
        functions.Add(Scalar("QUARTER", DateCategory, "QUARTER(date)", "Returns the quarter of a date (1 to 4).", 1, 1, Returns(DataFieldType.Integer), args => Date(args[0]) is DateTime date ? (long)((date.Month - 1) / 3 + 1) : null));
        functions.Add(Scalar("MONTH", DateCategory, "MONTH(date)", "Returns the month of a date (1 to 12).", 1, 1, Returns(DataFieldType.Integer), args => Date(args[0]) is DateTime date ? (long)date.Month : null));
        functions.Add(Scalar("DAY", DateCategory, "DAY(date)", "Returns the day of the month of a date.", 1, 1, Returns(DataFieldType.Integer), args => Date(args[0]) is DateTime date ? (long)date.Day : null));
        functions.Add(Scalar("WEEK", DateCategory, "WEEK(date)", "Returns the ISO week number of a date.", 1, 1, Returns(DataFieldType.Integer), args => Date(args[0]) is DateTime date ? (long)ISOWeek.GetWeekOfYear(date) : null));
        functions.Add(Scalar("WEEKDAY", DateCategory, "WEEKDAY(date)", "Returns the day of the week of a date, from 1 (Monday) to 7 (Sunday).", 1, 1, Returns(DataFieldType.Integer), args => Date(args[0]) is DateTime date ? (long)IsoDayOfWeek(date) : null));
        functions.Add(Scalar("HOUR", DateCategory, "HOUR(date)", "Returns the hour of a date-time (0 to 23).", 1, 1, Returns(DataFieldType.Integer), args => Date(args[0]) is DateTime date ? (long)date.Hour : null));
        functions.Add(Scalar("MINUTE", DateCategory, "MINUTE(date)", "Returns the minute of a date-time (0 to 59).", 1, 1, Returns(DataFieldType.Integer), args => Date(args[0]) is DateTime date ? (long)date.Minute : null));
        functions.Add(Scalar("DATE", DateCategory, "DATE(year, month, day) or DATE(value)", "Builds a date from its parts, or converts a value to a date.", 1, 3, Returns(DataFieldType.Date), MakeDate));
        functions.Add(Scalar("DATETIME", DateCategory, "DATETIME(value)", "Converts a value to a date-time.", 1, 1, Returns(DataFieldType.DateTime), args => Date(args[0])));
        functions.Add(Scalar("DATEADD", DateCategory, "DATEADD('part', number, date)", "Adds a number of years, quarters, months, weeks, days, hours, minutes, or seconds to a date.", 3, 3, types => types[2], args => DateAdd(Text(args[0]), Whole(args[1]), Date(args[2]))));
        functions.Add(Scalar("DATEDIFF", DateCategory, "DATEDIFF('part', start, end)", "Counts the years, quarters, months, weeks, days, hours, minutes, or seconds from start to end.", 3, 3, Returns(DataFieldType.Integer), args => DateDiff(Text(args[0]), Date(args[1]), Date(args[2]))));
        functions.Add(Scalar("DATETRUNC", DateCategory, "DATETRUNC('part', date)", "Truncates a date to the start of its year, quarter, month, week, day, hour, or minute.", 2, 2, types => types[1], args => DateTrunc(Text(args[0]), Date(args[1]))));
    }

    /// <summary>
    /// Returns the ISO day of the week, from 1 (Monday) to 7 (Sunday).
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>The day number.</returns>
    public static int IsoDayOfWeek(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)date.DayOfWeek;
    }

    /// <summary>
    /// Truncates a date to the start of a period.
    /// </summary>
    /// <param name="part">The period: year, quarter, month, week, day, hour, or minute.</param>
    /// <param name="date">The date.</param>
    /// <returns>The truncated date, or <see langword="null"/> when the part is unknown or the date is missing.</returns>
    public static DateTime? DateTrunc(string part, DateTime? date)
    {
        if (date is not DateTime value)
        {
            return null;
        }

        return NormalizePart(part) switch
        {
            "year" => new DateTime(value.Year, 1, 1, 0, 0, 0, value.Kind),
            "quarter" => new DateTime(value.Year, (value.Month - 1) / 3 * 3 + 1, 1, 0, 0, 0, value.Kind),
            "month" => new DateTime(value.Year, value.Month, 1, 0, 0, 0, value.Kind),
            "week" => value.Date.AddDays(1 - IsoDayOfWeek(value)),
            "day" => value.Date,
            "hour" => new DateTime(value.Year, value.Month, value.Day, value.Hour, 0, 0, value.Kind),
            "minute" => new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Kind),
            _ => null,
        };
    }

    private static DateTime? Date(object value)
    {
        return ExpressionOperations.ToDateTime(value);
    }

    private static object MakeDate(object[] args)
    {
        if (args.Length == 1)
        {
            return Date(args[0])?.Date;
        }

        if (args.Length != 3 || Whole(args[0]) is not int year || Whole(args[1]) is not int month || Whole(args[2]) is not int day)
        {
            return null;
        }

        if (year < 1 || year > 9999 || month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
        {
            return null;
        }

        return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
    }

    private static DateTime? DateAdd(string part, int? amount, DateTime? date)
    {
        if (amount is not int count || date is not DateTime value)
        {
            return null;
        }

        try
        {
            return NormalizePart(part) switch
            {
                "year" => value.AddYears(count),
                "quarter" => value.AddMonths(count * 3),
                "month" => value.AddMonths(count),
                "week" => value.AddDays(count * 7d),
                "day" => value.AddDays(count),
                "hour" => value.AddHours(count),
                "minute" => value.AddMinutes(count),
                "second" => value.AddSeconds(count),
                _ => null,
            };
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static long? DateDiff(string part, DateTime? start, DateTime? end)
    {
        if (start is not DateTime from || end is not DateTime to)
        {
            return null;
        }

        var months = (to.Year - from.Year) * 12 + to.Month - from.Month;

        return NormalizePart(part) switch
        {
            "year" => to.Year - from.Year,
            "quarter" => ((to.Year * 12) + to.Month - 1) / 3 - ((from.Year * 12) + from.Month - 1) / 3,
            "month" => months,
            "week" => (long)Math.Floor((to.Date - from.Date).TotalDays / 7),
            "day" => (long)(to.Date - from.Date).TotalDays,
            "hour" => (long)(to - from).TotalHours,
            "minute" => (long)(to - from).TotalMinutes,
            "second" => (long)(to - from).TotalSeconds,
            _ => null,
        };
    }

    private static string NormalizePart(string part)
    {
        var normalized = part?.Trim().ToLowerInvariant();

        return normalized switch
        {
            "years" or "yyyy" or "yy" => "year",
            "quarters" or "q" or "qq" => "quarter",
            "months" or "mm" or "m" => "month",
            "weeks" or "wk" or "ww" => "week",
            "days" or "dd" or "d" => "day",
            "hours" or "hh" => "hour",
            "minutes" or "mi" or "n" => "minute",
            "seconds" or "ss" or "s" => "second",
            _ => normalized,
        };
    }
}
