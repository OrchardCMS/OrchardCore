using OrchardVNext.Localization;
using System;

namespace OrchardVNext {
    public class OrchardFatalException : Exception {
        private readonly LocalizedString _localizedMessage;

        public OrchardFatalException(LocalizedString message)
            : base(message.Text) {
            _localizedMessage = message;
        }

        public OrchardFatalException(LocalizedString message, Exception innerException)
            : base(message.Text, innerException) {
            _localizedMessage = message;
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}
