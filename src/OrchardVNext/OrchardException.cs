using OrchardVNext.Localization;
using System;

namespace OrchardVNext {
    public class OrchardException : Exception {
        private readonly LocalizedString _localizedMessage;

        public OrchardException(LocalizedString message)
            : base(message.Text) {
            _localizedMessage = message;
        }

        public OrchardException(LocalizedString message, Exception innerException)
            : base(message.Text, innerException) {
            _localizedMessage = message;
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}