namespace OrchardCore.Setup.Services;

/// <summary>
/// Represents a contract for generating identifiers for setup users.
/// </summary>
public interface ISetupUserIdGenerator
{
    /// <summary>
    /// Generates a unique identifier for the setup user.
    /// </summary>
    string GenerateUniqueId();
}
