namespace OrchardCore.Setup.Services;

/// <summary>
/// Contract that represents a setup user id generator.
/// </summary>
public interface ISetupUserIdGenerator
{
    /// <summary>
    /// Generates a unique identifier for the setup user.
    /// </summary>
    string GenerateUniqueId();
}
