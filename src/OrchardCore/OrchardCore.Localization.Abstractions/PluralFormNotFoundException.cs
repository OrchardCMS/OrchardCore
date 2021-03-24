using System;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents an exception that is thrown when a pluralization form is not found.
    /// </summary>
    public class PluralFormNotFoundException : Exception
    {
        /// <summary>
        /// Creates new instance of <see cref="PluralFormNotFoundException"/>.
        /// </summary>
        public PluralFormNotFoundException()
        {

        }

        /// <summary>
        /// Creates new instance of <see cref="PluralFormNotFoundException"/> with a message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public PluralFormNotFoundException(string message) : base(message)
        {

        }

        /// <summary>
        /// Creates new instance of <see cref="PluralFormNotFoundException"/> with a message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="form">The <see cref="PluralForm"/> that causes the exception.</param>
        public PluralFormNotFoundException(string message, PluralForm form) : base(message)
        {
            Form = form;
        }

        /// <summary>
        /// Creates new instance of <see cref="PluralFormNotFoundException"/> with a message and inner exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public PluralFormNotFoundException(string message, Exception innerException) : base(message, innerException)
        {

        }

        /// <summary>
        /// Gets the pluralization form.
        /// </summary>
        public PluralForm Form { get; }
    }
}
