using System;

namespace OrchardCore.Data
{
    public static class DocumentStoreExceptions
    {
        public class ConcurrencyException : Exception
        {
            /// <summary>
            /// A concurrency exception thrown by an <see cref="ICommittableDocumentStore"/>.
            /// </summary>
            public ConcurrencyException(string message) : this(message, exception: null)
            {
            }

            /// <summary>
            /// A concurrency exception thrown by an <see cref="ICommittableDocumentStore"/>.
            /// </summary>
            public ConcurrencyException(string message, Exception exception) : base(message, exception)
            {
            }
        }
    }
}
