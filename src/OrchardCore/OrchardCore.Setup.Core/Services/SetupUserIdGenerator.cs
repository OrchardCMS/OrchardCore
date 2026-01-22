using OrchardCore.Entities;

namespace OrchardCore.Setup.Services;

/// <summary>
/// Provides functionality to generate unique identifiers for setup users using a specified ID generator implementation.
/// </summary>
/// <remarks>This class acts as an adapter for the <see cref="IIdGenerator"/> interface, allowing setup user IDs
/// to be generated consistently across different environments. Thread safety and uniqueness guarantees depend on the
/// underlying <see cref="IIdGenerator"/> implementation.</remarks>
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
