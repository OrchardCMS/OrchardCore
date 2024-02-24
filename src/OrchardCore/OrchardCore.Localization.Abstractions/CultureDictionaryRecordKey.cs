using System;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a key for <see cref="CultureDictionaryRecord"/>.
    /// </summary>
    public readonly struct CultureDictionaryRecordKey : IEquatable<CultureDictionaryRecordKey>
    {
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
            MessageId = messageId;
            Context = context;
        }

        /// <summary>
        /// Gets the message Id.
        /// </summary>
        public string MessageId { get; }

        /// <summary>
        /// Gets the message context.
        /// </summary>
        public string Context { get; }

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
            => string.Equals(MessageId, other.MessageId) && String.Equals(Context, other.Context);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(MessageId, Context);

        public override string ToString()
            => string.IsNullOrEmpty(Context)
                ? MessageId
                : Context.ToLowerInvariant() + "|" + MessageId;

        public static bool operator ==(CultureDictionaryRecordKey left, CultureDictionaryRecordKey right) => left.Equals(right);

        public static bool operator !=(CultureDictionaryRecordKey left, CultureDictionaryRecordKey right) => !(left == right);
    }
}
