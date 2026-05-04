namespace OrchardCore.Setup.Services;

/// <summary>
/// Defines a contract for generating unique identifier strings for use as keys, tokens, or other distinct values.
/// </summary>
/// <remarks>Implementations may generate identifiers using different algorithms or formats. The generated
/// identifier is intended to be unique within the context of the application or system. This interface is typically
/// used during setup or initialization processes where a reliable, non-colliding identifier is required.</remarks>
public interface ISetupUserIdGenerator
{
    /// <summary>
    /// Generates a unique identifier string suitable for use as a key or token.
    /// </summary>
    /// <returns>A string containing a unique identifier. The format and length of the identifier may vary depending on the
    /// implementation.
    /// </returns>
    string GenerateUniqueId();
}
