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
}
