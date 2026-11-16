using System.Diagnostics.CodeAnalysis;

namespace OrchardCore.Secrets;

/// <summary>
/// Represents a secret that can be stored and retrieved securely. A secret is either a <see cref="TextSecret"/>,
/// an <see cref="RsaKeySecret"/> or an <see cref="X509Secret"/>.
/// </summary>
/// <remarks>
/// A secret that is not set, for instance when a secret is not found, has a <see langword="null"/>
/// <see cref="Value"/> and matches the <see langword="null"/> pattern.
/// </remarks>
public union Secret(TextSecret, RsaKeySecret, X509Secret)
{
    /// <summary>
    /// Gets the types of the secrets.
    /// </summary>
    public static IReadOnlyList<Type> Types { get; } = [typeof(TextSecret), typeof(RsaKeySecret), typeof(X509Secret)];

    /// <summary>
    /// Creates a secret from an instance of one of the secret types.
    /// </summary>
    /// <param name="value">An instance of one of the <see cref="Types"/>, or <see langword="null"/>.</param>
    /// <returns>The secret holding <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not an instance of a secret type.</exception>
    public static Secret FromValue(object value) => value switch
    {
        null => default,
        TextSecret text => text,
        RsaKeySecret rsaKey => rsaKey,
        X509Secret x509 => x509,
        _ => throw new ArgumentException($"The type '{value.GetType().FullName}' is not a secret type.", nameof(value)),
    };

    /// <summary>
    /// Finds the type of the secrets from its name or its full name.
    /// </summary>
    /// <param name="name">The name or the full name of the type, e.g. <c>TextSecret</c>.</param>
    /// <param name="type">The secret type, when one is found.</param>
    /// <returns><see langword="true"/> if <paramref name="name"/> designates a secret type.</returns>
    public static bool TryGetType(string name, [NotNullWhen(true)] out Type type)
    {
        foreach (var secretType in Types)
        {
            if (secretType.Name == name || secretType.FullName == name)
            {
                type = secretType;
                return true;
            }
        }

        type = null;
        return false;
    }
}
