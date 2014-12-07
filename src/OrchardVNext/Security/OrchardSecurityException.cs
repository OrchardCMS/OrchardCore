using System;
using OrchardVNext.Localization;

namespace OrchardVNext.Security {
    public class OrchardSecurityException : OrchardCoreException {
        public OrchardSecurityException(LocalizedString message) : base(message) { }
        public OrchardSecurityException(LocalizedString message, Exception innerException) : base(message, innerException) { }

        public string PermissionName { get; set; }
    }
}
