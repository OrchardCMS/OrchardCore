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
        private readonly string _key;

        /// <summary>
        /// Creates new instance of <see cref="CultureDictionaryRecordKey"/>.
        /// </summary>
        /// <param name="messageId">The message Id.</param>
        public CultureDictionaryRecordKey(string messageId) : this(messageId, null)
        {

        }

        /// <summary>
        /// Creates new instance of <see cref="CultureDictionaryRecordKey"/>.
        /// </summary>
        /// <param name="messageId">The message Id.</param>
        /// <param name="context">The message context.</param>
        public CultureDictionaryRecordKey(string messageId, string context)
        {
            _messageId = messageId;
            _context = context;

            _key = String.IsNullOrEmpty(context)
                ? messageId
                : context.ToLowerInvariant() + "|" + messageId;
        }

        public static implicit operator string(CultureDictionaryRecordKey cultureDictionaryRecordKey)
            => cultureDictionaryRecordKey._key;

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
        public bool Equals(CultureDictionaryRecordKey other) => String.Equals(_key, other._key);

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
    }
}
