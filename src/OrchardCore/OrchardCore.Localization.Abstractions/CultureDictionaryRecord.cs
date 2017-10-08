using System;

namespace OrchardCore.Localization
{
    public class CultureDictionaryRecord
    {
        public string Key { get; }
 
        public string[] Translations { get; }

        public CultureDictionaryRecord(string messageId, string context, string[] translations)
        {
            Key = GetKey(messageId, context);
            Translations = translations;
        }

        public static string GetKey(string messageId, string context)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentException("MessageId can't be empty.", nameof(messageId));
            }

            if (string.IsNullOrEmpty(context))
            {
                return messageId;
            }

            return context.ToLowerInvariant() + "|" + messageId;
        }
    }
}
