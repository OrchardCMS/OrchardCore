using System;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a key for <see cref="CultureDictionaryRecord"/>.
    /// </summary>
    public struct CultureDictionaryRecordKey : IEquatable<CultureDictionaryRecordKey>
    {
        private readonly string _messageId;
        private readonly string _context;

        /// <summary>
        /// Creates new instance of <see cref="CultureDictionaryRecordKey"/>.
        /// </summary>
        /// <param name="messageId">The message Id.</param>
        /// <param name="context">The message context.</param>
        public CultureDictionaryRecordKey(string messageId, string context)
        {
            _messageId = messageId;
            _context = context;

            if (String.IsNullOrEmpty(context))
            {
                Key = messageId;
            }

            Key = String.IsNullOrEmpty(context) ? messageId : context.ToLowerInvariant() + "|" + messageId;
        }

        private string Key { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is CultureDictionaryRecordKey other)
            {
                return Equals(other);
            }

            return false;
        }

        /// <inheritdoc />
        public bool Equals(CultureDictionaryRecordKey other) => String.Equals(Key, other.Key);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 13;
                hashCode = (hashCode * 397) ^ (!String.IsNullOrEmpty(_messageId) ? _messageId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (!String.IsNullOrEmpty(_context) ? _context.GetHashCode() : 0);

                return hashCode;
            }
        }

        public override string ToString() => Key;
    }
}
