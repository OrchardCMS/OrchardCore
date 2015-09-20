using System;
using Microsoft.Framework.Localization;

namespace Orchard.Security {
    public class OrchardSecurityException : OrchardCoreException {
        public OrchardSecurityException(LocalizedString message) : base(message) { }
        public OrchardSecurityException(LocalizedString message, Exception innerException) : base(message, innerException) { }

        public string PermissionName { get; set; }
    }
}
