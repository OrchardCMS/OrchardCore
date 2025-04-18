namespace OrchardCore.Setup.Services;

/// <summary>
/// Contract that represents a setup user id generator that will be used during the setup process.
/// </summary>
public interface ISetupUserIdGenerator
{
    /// <summary>
    /// Generates a unique identifier for the setup user.
    /// </summary>
    string GenerateUniqueId();
}
