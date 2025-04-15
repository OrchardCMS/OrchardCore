using OrchardCore.Entities;

namespace OrchardCore.Setup.Services;

/// <summary>
/// Represents a class that generates unique user IDs for setup.
/// </summary>
public class SetupUserIdGenerator : ISetupUserIdGenerator
{
    private readonly IIdGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetupUserIdGenerator"/> class.
    /// </summary>
    /// <param name="generator"></param>
    public SetupUserIdGenerator(IIdGenerator generator) => _generator = generator;

    /// <inheritdoc/>
    public string GenerateUniqueId() => _generator.GenerateUniqueId();
}
