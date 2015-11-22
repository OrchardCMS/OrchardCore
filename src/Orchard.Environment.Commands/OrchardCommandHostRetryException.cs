using Microsoft.Extensions.Localization;
using System;

namespace Orchard.Environment.Commands
{
    public class OrchardCommandHostRetryException : OrchardCoreException
    {
        public OrchardCommandHostRetryException(LocalizedString message)
            : base(message)
        {
        }

        public OrchardCommandHostRetryException(LocalizedString message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}