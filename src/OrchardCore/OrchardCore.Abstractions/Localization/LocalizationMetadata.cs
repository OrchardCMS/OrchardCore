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
    /// Declares the range of values the count of a plural message can take, so that the extracted template carries
    /// the gettext <c>range</c> flag, for example <c>S.Plural(Range(0, 23, hours), "1 hour", "{0} hours")</c>. It can be
    /// combined with <see cref="Arg{T}(string, T)"/>, for example <c>Arg("hours", Range(0, 23, hours))</c>.
    /// </summary>
    /// <param name="min">The smallest value of the count. Must be a non-negative compile-time constant to be extracted.</param>
    /// <param name="max">The largest value of the count. Must be a compile-time constant, not smaller than
    /// <paramref name="min"/>, to be extracted.</param>
    /// <param name="count">The count of the plural message.</param>
    /// <returns><paramref name="count"/>, unchanged.</returns>
    public static int Range(int min, int max, int count)
    {
        _ = min;
        _ = max;

        return count;
    }
}
