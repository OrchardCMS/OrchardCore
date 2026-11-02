namespace OrchardCore.Localization;

/// <summary>
/// Represents a record in a <see cref="CultureDictionary"/>.
/// </summary>
public class CultureDictionaryRecord
{
    /// <summary>
    /// Creates new instance of <see cref="CultureDictionaryRecord"/>.
    /// </summary>
    /// <param name="messageId">The message Id.</param>
    /// <param name="translations">a list of translations.</param>
    public CultureDictionaryRecord(string messageId, params string[] translations)
        : this(messageId, null, translations)
    {

    }

    /// <summary>
    /// Creates new instance of <see cref="CultureDictionaryRecord"/>.
    /// </summary>
    /// <param name="messageId">The message Id.</param>
    /// <param name="context">The message context.</param>
    /// <param name="translations">a list of translations.</param>
    public CultureDictionaryRecord(string messageId, string context, string[] translations)
        : this(messageId, context, translations, null)
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="CultureDictionaryRecord"/>.
    /// </summary>
    /// <param name="messageId">The message Id.</param>
    /// <param name="context">The message context.</param>
    /// <param name="translations">a list of translations.</param>
    /// <param name="pluralRule">The pluralization rule of the translations, or <c>null</c> to use the rule of the culture.</param>
    public CultureDictionaryRecord(string messageId, string context, string[] translations, PluralizationRuleDelegate pluralRule)
    {
        Key = GetKey(messageId, context);
        Translations = translations;
        PluralRule = pluralRule;
    }

    /// <summary>
    /// Gets the resource key.
    /// </summary>
    public CultureDictionaryRecordKey Key { get; }

    /// <summary>
    /// Gets the translation values.
    /// </summary>
    public string[] Translations { get; }

    /// <summary>
    /// Gets the pluralization rule selecting the plural form of the translations, e.g. the one defined
    /// by the <c>Plural-Forms</c> header of the PO file of the record, or <c>null</c> to use the rule of the culture.
    /// </summary>
    public PluralizationRuleDelegate PluralRule { get; }

    /// <summary>
    /// Retrieved the resource key using <paramref name="messageId"/> and <paramref name="context"/>.
    /// </summary>
    /// <param name="messageId">The message Id.</param>
    /// <param name="context">The message context.</param>
    /// <returns>The resource key.</returns>
    public static CultureDictionaryRecordKey GetKey(string messageId, string context)
    {
        if (string.IsNullOrEmpty(messageId))
        {
            throw new ArgumentException("MessageId can't be empty.", nameof(messageId));
        }

        return new CultureDictionaryRecordKey
        {
            MessageId = messageId,
            Context = context,
        };
    }
}
