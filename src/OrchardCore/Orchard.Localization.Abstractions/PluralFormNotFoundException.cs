using System;

namespace Orchard.Localization.Abstractions
{
    public class PluralFormNotFoundException : Exception
    {
        public PluralFormNotFoundException()
        {
        }

        public PluralFormNotFoundException(string message) : base(message)
        {
        }

        public PluralFormNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
