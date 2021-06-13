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
        }

        public static implicit operator string(CultureDictionaryRecordKey cultureDictionaryRecordKey)
            => cultureDictionaryRecordKey.ToString();

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
        public bool Equals(CultureDictionaryRecordKey other)
            => String.Equals(_messageId, other._messageId) && String.Equals(_context, other._context);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(_messageId, _context);

        public override string ToString()
            => String.IsNullOrEmpty(_context)
                ? _messageId
                : _context.ToLowerInvariant() + "|" + _messageId;
    }
}
