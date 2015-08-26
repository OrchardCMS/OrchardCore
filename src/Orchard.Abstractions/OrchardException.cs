using System;
using Microsoft.Framework.Localization;

namespace Orchard.Abstractions {
    public class OrchardException : Exception {
        private readonly LocalizedString _localizedMessage;

        public OrchardException(LocalizedString message)
            : base(message) {
            _localizedMessage = message;
        }

        public OrchardException(LocalizedString message, Exception innerException)
            : base(message, innerException) {
            _localizedMessage = message;
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}