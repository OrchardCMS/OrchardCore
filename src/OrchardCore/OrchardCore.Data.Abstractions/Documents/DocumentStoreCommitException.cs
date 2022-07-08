using System;

namespace OrchardCore.Data.Documents
{
    /// <summary>
    /// The <see cref="Exception"/> that is thrown if <see cref="IDocumentStore.CommitAsync"/> fails.
    /// </summary>
    public class DocumentStoreCommitException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="DocumentStoreCommitException"/> with the specified
        /// exception message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DocumentStoreCommitException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="DocumentStoreCommitException"/> with the specified
        /// exception message and inner exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public DocumentStoreCommitException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
