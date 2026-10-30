namespace OrchardCore.Localization;

/// <summary>
/// Provides helpers that describe localized strings to translators. They are read by the localization extraction and
/// have no effect at runtime.
/// </summary>
public static class LocalizationMetadata
{
    /// <summary>
    /// Names a format argument of a localized string, so that the extracted template explains the meaning of its
    /// placeholder to translators, for example <c>S["Hello {0}", Arg("user name", user.Name)]</c>.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="name">The name describing the argument. Must be a compile-time constant to be extracted.</param>
    /// <param name="value">The value of the argument.</param>
    /// <returns><paramref name="value"/>, unchanged.</returns>
    public static T Arg<T>(string name, T value)
    {
        _ = name;

        return value;
    }

    /// <summary>
    /// Names the count of a plural message and declares the range of values it can take, so that the extracted
    /// template also carries the gettext <c>range</c> flag, for example
    /// <c>S.Plural(Arg("hours", hours, 0, 23), "1 hour", "{0} hours")</c>.
    /// </summary>
    /// <param name="name">The name describing the count. Must be a compile-time constant to be extracted.</param>
    /// <param name="value">The count of the plural message.</param>
    /// <param name="min">The smallest value of the count. Must be a non-negative compile-time constant to be extracted.</param>
    /// <param name="max">The largest value of the count. Must be a compile-time constant, not smaller than
    /// <paramref name="min"/>, to be extracted.</param>
    /// <returns><paramref name="value"/>, unchanged.</returns>
    public static int Arg(string name, int value, int min, int max)
    {
        _ = name;
        _ = min;
        _ = max;

        return value;
    }
}
