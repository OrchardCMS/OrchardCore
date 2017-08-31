using System;

namespace Orchard.Localization
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
