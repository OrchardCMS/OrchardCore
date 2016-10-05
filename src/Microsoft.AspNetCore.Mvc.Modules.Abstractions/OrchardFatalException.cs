using System;
using Microsoft.Extensions.Localization;

namespace Orchard
{
    public class OrchardFatalException : Exception
    {
        private readonly LocalizedString _localizedMessage;

        public OrchardFatalException(LocalizedString message)
            : base(message)
        {
            _localizedMessage = message;
        }

        public OrchardFatalException(LocalizedString message, Exception innerException)
            : base(message, innerException)
        {
            _localizedMessage = message;
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}