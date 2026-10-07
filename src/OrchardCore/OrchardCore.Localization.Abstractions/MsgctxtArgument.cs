namespace OrchardCore.Localization;

/// <summary>
/// Represents a message context argument.
/// </summary>
public readonly struct MsgctxtArgument
{
    /// <summary>
    /// Creates a new instance of <see cref="MsgctxtArgument"/>.
    /// </summary>
    /// <param name="context">The message context.</param>
    public MsgctxtArgument(string context)
    {
        Context = context;
    }

    /// <summary>
    /// Gets the message context.
    /// </summary>
    public string Context { get; }
}
