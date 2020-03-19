using System;

namespace OrchardCore.Data.Documents
{
    public class DocumentStoreConcurrencyException : Exception
    {
        /// <summary>
        /// A concurrency exception thrown by an <see cref="ICommittableDocumentStore"/>.
        /// </summary>
        public DocumentStoreConcurrencyException(string message) : this(message, exception: null)
        {
        }

        /// <summary>
        /// A concurrency exception thrown by an <see cref="ICommittableDocumentStore"/>.
        /// </summary>
        public DocumentStoreConcurrencyException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}
