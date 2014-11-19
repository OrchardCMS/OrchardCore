using OrchardVNext.Localization;
using System;

namespace OrchardVNext {
    public class OrchardCoreException : Exception {
        private readonly LocalizedString _localizedMessage;

        public OrchardCoreException(LocalizedString message)
            : base(message.Text) {
            _localizedMessage = message;
        }

        public OrchardCoreException(LocalizedString message, Exception innerException)
            : base(message.Text, innerException) {
            _localizedMessage = message;
        }

        public LocalizedString LocalizedMessage { get { return _localizedMessage; } }
    }
}