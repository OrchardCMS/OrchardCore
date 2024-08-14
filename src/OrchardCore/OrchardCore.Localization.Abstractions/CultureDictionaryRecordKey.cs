namespace OrchardCore.Localization;

/// <summary>
/// Represents a key for <see cref="CultureDictionaryRecord"/>.
/// </summary>
public readonly record struct CultureDictionaryRecordKey
{
    /// <summary>
    /// Gets the message Id.
    /// </summary>
    public required string MessageId { get; init; }

    /// <summary>
    /// Gets the message context.
    /// </summary>
    public string Context { get; init; }

    public static implicit operator string(CultureDictionaryRecordKey cultureDictionaryRecordKey)
        => cultureDictionaryRecordKey.ToString();

    public override string ToString()
        => string.IsNullOrEmpty(Context)
            ? MessageId
            : $"{Context.ToLowerInvariant()}|{MessageId}";
}
